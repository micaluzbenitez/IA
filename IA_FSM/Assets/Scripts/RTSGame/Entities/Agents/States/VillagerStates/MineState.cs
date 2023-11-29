using System;
using System.Collections.Generic;
using UnityEngine;
using Toolbox;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class MineState : State
    {
        private Timer mineTimer = new Timer();

        private GoldMine goldMine;
        private int goldQuantity;
        private int totalGoldsRecolected;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Villager villager = parameters[0] as Villager;
            float timePerMine = Convert.ToSingle(parameters[1]);
            int maxGoldRecolected = Convert.ToInt32(parameters[2]);
            int goldsPerFood = Convert.ToInt32(parameters[3]);
            float deltaTime = Convert.ToSingle(parameters[4]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!mineTimer.Active) mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE, true);
                UpdateMineTimer(villager, deltaTime, maxGoldRecolected, goldsPerFood);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            Voronoi voronoi = parameters[0] as Voronoi;
            Villager villager = parameters[1] as Villager;
            int goldsPerFood = Convert.ToInt32(parameters[2]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += TakeRefuge;
                goldMine = voronoi.GetMineCloser(villager.Position);

                // Checks when returns to take refuge state
                if (villager.ReturnsToTakeRefuge)
                {
                    if (Vector2.Distance(villager.Position, goldMine.Position) > 1f) Transition((int)FSM_Villager_Flags.OnGoMine);
                    villager.ReturnsToTakeRefuge = false;
                }
                if (villager.NeedsFood) Transition((int)FSM_Villager_Flags.OnGoEat);

                villager.ReturnsToTakeRefuge = false;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= TakeRefuge;
                mineTimer.DesactiveTimer();
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void UpdateMineTimer(Villager villager, float deltaTime, int maxGoldRecolected, int goldsPerFood)
        {
            if (mineTimer.Active) mineTimer.UpdateTimer(deltaTime);
            if (mineTimer.ReachedTimer()) Mine(villager, maxGoldRecolected, goldsPerFood);
        }

        private void Mine(Villager villager, int maxGoldRecolected, int goldsPerFood)
        {
            if (goldMine && goldMine.ConsumeGold())
            {
                goldQuantity++;
                villager.GoldQuantityText = goldQuantity.ToString();

                totalGoldsRecolected++;

                if (goldQuantity == maxGoldRecolected) // Guardar oro
                {
                    goldQuantity = 0;
                    goldMine.RemoveVillager();
                    Transition((int)FSM_Villager_Flags.OnGoSaveMaterials);
                }
                else if (totalGoldsRecolected % goldsPerFood == 0) // Comer
                {
                    villager.NeedsFood = true;
                    Transition((int)FSM_Villager_Flags.OnGoEat);
                }
                else // Continuar minando
                {
                    mineTimer.ActiveTimer();
                }
            }
            else
            {
                if (goldMine) goldMine.RemoveVillager();
                Transition((int)FSM_Villager_Flags.OnGoMine);
            }
        }

        private void TakeRefuge()
        {
            if (goldMine) goldMine.RemoveVillager();
            Transition((int)FSM_Villager_Flags.OnTakingRefuge);
        }
    }
}