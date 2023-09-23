using System;
using System.Collections.Generic;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class EatState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            GoldMine goldMine = parameters[0] as GoldMine;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (goldMine.ConsumeFood())
                {
                    Transition((int)FSM_Villager_Flags.OnMining);
                }
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