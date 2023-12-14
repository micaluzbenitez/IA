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
        private int goldQuantity;

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Caravan caravan = stateParameters.Parameters[2] as Caravan;
            int foodPerTravel = Convert.ToInt32(stateParameters.Parameters[4]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (goldMine && goldMine.BeingUsed)
                {
                    // Take villagers gold
                    for (int i = 0; i < goldMine.Villagers.Count; i++)
                    {
                        goldQuantity += int.Parse(goldMine.Villagers[i].GoldQuantityText);
                        goldMine.Villagers[i].GoldQuantityText = "0";
                    }
                    caravan.GoldQuantityText = goldQuantity.ToString();
                    goldQuantity = 0;

                    // Deliver food
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

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            Voronoi voronoi = stateParameters.Parameters[1] as Voronoi;
            Caravan caravan = stateParameters.Parameters[2] as Caravan;

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