using System;
using System.Collections.Generic;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.CaravanStates
{
    public class DeliverMineState : State
    {
        public static Action OnDeliverFood;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            GoldMine goldMine = parameters[0] as GoldMine;
            int foodQuantity = Convert.ToInt32(parameters[1]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                goldMine.DeliverFood(foodQuantity);
                OnDeliverFood?.Invoke();
                Transition((int)FSM_Caravan_Flags.OnGoTakeFood);
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