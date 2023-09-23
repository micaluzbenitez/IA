using System;
using System.Collections.Generic;
using UnityEngine;
using Toolbox;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

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
            goldMine = parameters[0] as GoldMine;
            float timePerMine = Convert.ToSingle(parameters[1]);
            int maxGoldRecolected = Convert.ToInt32(parameters[2]);
            int goldsPerFood = Convert.ToInt32(parameters[3]);
            goldText = parameters[4] as TextMesh;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!mineTimer.Active) mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE, true);
                UpdateMineTimer(goldMine, maxGoldRecolected, goldsPerFood);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                mineTimer.DesactiveTimer();
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void UpdateMineTimer(GoldMine goldMine, int maxGoldRecolected, int goldsPerFood)
        {
            if (mineTimer.Active) mineTimer.UpdateTimer();

            if (mineTimer.ReachedTimer())
            {
                if (goldMine.ConsumeGold())
                {
                    goldQuantity++;
                    goldText.text = goldQuantity.ToString();

                    totalGoldsRecolected++;

                    if (goldQuantity == maxGoldRecolected) // Save golds
                    {
                        goldQuantity = 0;
                        goldMine.RemoveVillager();
                        Transition((int)FSM_Villager_Flags.OnGoSaveMaterials);
                    }
                    else if (totalGoldsRecolected % goldsPerFood == 0) // Eat
                    {
                        Transition((int)FSM_Villager_Flags.OnGoEat);
                    }
                    else // Continue mining
                    {
                        mineTimer.ActiveTimer();
                    }
                }
                else
                {
                    goldMine.RemoveVillager();
                    Transition((int)FSM_Villager_Flags.OnGoMine);
                }
            }
        }
    }
}