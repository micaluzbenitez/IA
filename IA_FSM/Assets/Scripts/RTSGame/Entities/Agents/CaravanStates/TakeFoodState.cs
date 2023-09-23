using System;
using System.Collections.Generic;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.CaravanStates
{
    public class TakeFoodState : State
    {
        public static Action OnTakeFood;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                OnTakeFood?.Invoke();
                Transition((int)FSM_Caravan_Flags.OnGoMine);
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
    }
}