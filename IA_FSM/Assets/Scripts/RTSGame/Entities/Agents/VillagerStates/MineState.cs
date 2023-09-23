using System;
using System.Collections.Generic;
using Toolbox;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class MineState : State
    {
        private Timer mineTimer = new Timer();

        public static Action OnMine;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            GoldMine goldMine = parameters[0] as GoldMine;
            float timePerMine = Convert.ToSingle(parameters[1]);
            int goldQuantity = Convert.ToInt32(parameters[2]);
            int maxGoldRecolected = Convert.ToInt32(parameters[3]);
            int goldsPerFood = Convert.ToInt32(parameters[4]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!mineTimer.Active) mineTimer.SetTimer(timePerMine, Timer.TIMER_MODE.DECREASE, true);
                UpdateMineTimer(goldMine, goldQuantity, maxGoldRecolected, goldsPerFood);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void UpdateMineTimer(GoldMine goldMine, int goldQuantity, int maxGoldRecolected, int goldsPerFood)
        {
            if (mineTimer.Active) mineTimer.UpdateTimer();

            if (mineTimer.ReachedTimer())
            {
                if (goldMine.ConsumeGold())
                {
                    OnMine?.Invoke();

                    if ((goldQuantity + 1) == maxGoldRecolected) Transition((int)FSM_Villager_Flags.OnGoSaveMaterials);
                    else if ((goldQuantity + 1) % goldsPerFood == 0) Transition((int)FSM_Villager_Flags.OnGoEat);
                    else mineTimer.ActiveTimer();
                }
                else
                {
                    Transition((int)FSM_Villager_Flags.OnGoMine);
                }
            }
        }
    }
}