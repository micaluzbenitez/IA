using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class DeliverMineState : State
    {
        private GoldMine goldMine;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Caravan caravan = parameters[0] as Caravan;
            int foodPerTravel = Convert.ToInt32(parameters[1]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (goldMine && goldMine.BeingUsed)
                {
                    goldMine.DeliverFood(foodPerTravel);
                    caravan.FoodQuantityText = "0";
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
            Voronoi voronoi = parameters[0] as Voronoi;
            Caravan caravan = parameters[1] as Caravan;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
                goldMine = voronoi.GetMineCloser(caravan.Position);

                // Check when returns to take refuge state
                if (caravan.ReturnsToTakeRefuge)
                {
                    if (Vector2.Distance(caravan.Position, goldMine.Position) > 1f) Transition((int)FSM_Villager_Flags.OnGoMine);
                    caravan.ReturnsToTakeRefuge = false;
                }

                caravan.ReturnsToTakeRefuge = false;
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