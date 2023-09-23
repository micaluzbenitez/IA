using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.CaravanStates
{
    public class TakeFoodState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            int foodPerTravel = Convert.ToInt32(parameters[0]);
            TextMesh foodText = parameters[1] as TextMesh;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                foodText.text = foodPerTravel.ToString();
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