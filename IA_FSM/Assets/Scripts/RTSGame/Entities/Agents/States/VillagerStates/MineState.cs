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

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;
            float timePerMine = Convert.ToSingle(stateParameters.Parameters[4]);
            int maxGoldRecolected = Convert.ToInt32(stateParameters.Parameters[5]);
            int goldsPerFood = Convert.ToInt32(stateParameters.Parameters[6]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!mineTimer.Active) mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE, true);
                UpdateMineTimer(villager, maxGoldRecolected, goldsPerFood);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            Voronoi voronoi = stateParameters.Parameters[1] as Voronoi;
            Villager villager = stateParameters.Parameters[2] as Villager;
            int goldsPerFood = Convert.ToInt32(stateParameters.Parameters[6]);

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

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
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

        private void UpdateMineTimer(Villager villager, int maxGoldRecolected, int goldsPerFood)
        {
            if (mineTimer.Active) mineTimer.UpdateTimer(villager.DeltaTime);
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