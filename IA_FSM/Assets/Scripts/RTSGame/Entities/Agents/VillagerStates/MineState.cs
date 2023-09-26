using System;
using System.Collections.Generic;
using UnityEngine;
using Toolbox;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class MineState : State
    {
        private Timer mineTimer = new Timer();

        private GoldMine goldMine;
        private int goldQuantity;
        private int totalGoldsRecolected;
        private TextMesh goldText;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            float timePerMine = Convert.ToSingle(parameters[0]);
            int maxGoldRecolected = Convert.ToInt32(parameters[1]);
            int goldsPerFood = Convert.ToInt32(parameters[2]);
            goldText = parameters[3] as TextMesh;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!mineTimer.Active) mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE, true);
                UpdateMineTimer(maxGoldRecolected, goldsPerFood);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            Voronoi voronoi = parameters[1] as Voronoi;
            int goldsPerFood = Convert.ToInt32(parameters[2]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += TakeRefuge;
                goldMine = voronoi.GetMineCloser(transform.position);

                // Checks when returns to take refuge state
                if (Vector2.Distance(transform.position, goldMine.transform.position) > 1f) Transition((int)FSM_Villager_Flags.OnGoMine);
                if (totalGoldsRecolected != 0 && totalGoldsRecolected % goldsPerFood == 0)
                {
                    totalGoldsRecolected = 0;
                    Transition((int)FSM_Villager_Flags.OnGoEat);
                }
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

        private void UpdateMineTimer(int maxGoldRecolected, int goldsPerFood)
        {
            if (mineTimer.Active) mineTimer.UpdateTimer();
            if (mineTimer.ReachedTimer()) Mine(maxGoldRecolected, goldsPerFood);
        }

        private void Mine(int maxGoldRecolected, int goldsPerFood)
        {
            if (goldMine && goldMine.ConsumeGold())
            {
                goldQuantity++;
                goldText.text = goldQuantity.ToString();

                totalGoldsRecolected++;

                if (goldQuantity == maxGoldRecolected) // Guardar oro
                {
                    goldQuantity = 0;
                    goldMine.RemoveVillager();
                    Transition((int)FSM_Villager_Flags.OnGoSaveMaterials);
                }
                else if (totalGoldsRecolected % goldsPerFood == 0) // Comer
                {
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