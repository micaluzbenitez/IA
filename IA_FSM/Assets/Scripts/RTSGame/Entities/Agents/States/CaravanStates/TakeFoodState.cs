using System;
using System.Collections.Generic;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class TakeFoodState : State
    {
        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Caravan caravan = stateParameters.Parameters[2] as Caravan;
            int foodPerTravel = Convert.ToInt32(stateParameters.Parameters[4]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                // Deliver gold
                caravan.UrbanCenter.DeliverGold(int.Parse(caravan.GoldQuantityText));
                caravan.GoldQuantityText = "0";

                // Deliver food
                caravan.FoodQuantityText = foodPerTravel.ToString();
                Transition((int)FSM_Caravan_Flags.OnGoMine);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            Caravan caravan = stateParameters.Parameters[2] as Caravan;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
                caravan.ReturnsToTakeRefuge = false;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }
    }
}