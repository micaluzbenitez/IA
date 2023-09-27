using System;
using System.Collections.Generic;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class TakeFoodState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Caravan caravan = parameters[0] as Caravan;
            int foodPerTravel = Convert.ToInt32(parameters[1]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                caravan.FoodQuantityText = foodPerTravel.ToString();
                Transition((int)FSM_Caravan_Flags.OnGoMine);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
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