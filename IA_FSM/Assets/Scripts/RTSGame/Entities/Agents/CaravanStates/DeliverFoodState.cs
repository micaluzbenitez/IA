using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.CaravanStates
{
    public class DeliverMineState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            GoldMine goldMine = parameters[0] as GoldMine;
            int foodPerTravel = Convert.ToInt32(parameters[1]);
            TextMesh foodText = parameters[2] as TextMesh;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                goldMine.DeliverFood(foodPerTravel);
                foodText.text = "0";
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