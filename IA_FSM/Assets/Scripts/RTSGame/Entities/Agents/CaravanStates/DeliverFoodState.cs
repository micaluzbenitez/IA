using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.CaravanStates
{
    public class DeliverMineState : State
    {
        private GoldMine goldMine;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            int foodPerTravel = Convert.ToInt32(parameters[0]);
            TextMesh foodText = parameters[1] as TextMesh;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (goldMine && goldMine.BeingUsed)
                {
                    goldMine.DeliverFood(foodPerTravel);
                    foodText.text = "0";
                    Transition((int)FSM_Caravan_Flags.OnGoTakeFood);
                }
                else
                {
                    Transition((int)FSM_Caravan_Flags.OnGoMine);
                }
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            Voronoi voronoi = parameters[1] as Voronoi;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
                goldMine = voronoi.GetMineCloser(transform.position);

                // Check when returns to take refuge state
                if (Vector2.Distance(transform.position, goldMine.transform.position) > 1f) Transition((int)FSM_Villager_Flags.OnGoMine);
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