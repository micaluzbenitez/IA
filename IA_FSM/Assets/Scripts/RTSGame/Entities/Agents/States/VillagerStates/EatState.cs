using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class EatState : State
    {
        private GoldMine goldMine;

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (goldMine && goldMine.ConsumeFood())
                {
                    villager.NeedsFood = false;
                    Transition((int)FSM_Villager_Flags.OnMining);
                }
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            Voronoi voronoi = stateParameters.Parameters[1] as Voronoi;
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += TakeRefuge;
                goldMine = voronoi.GetMineCloser(villager.Position);

                // Check when returns to take refuge state
                if (villager.ReturnsToTakeRefuge)
                {
                    if (Vector2.Distance(villager.Position, goldMine.Position) > 1f) Transition((int)FSM_Villager_Flags.OnGoMine);
                    villager.ReturnsToTakeRefuge = false;
                }

                villager.ReturnsToTakeRefuge = false;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= TakeRefuge;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void TakeRefuge()
        {
            if (goldMine) goldMine.RemoveVillager();
            Transition((int)FSM_Villager_Flags.OnTakingRefuge);
        }
    }
}