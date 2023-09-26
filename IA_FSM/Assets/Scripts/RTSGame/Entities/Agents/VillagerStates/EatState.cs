using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class EatState : State
    {
        private GoldMine goldMine;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (goldMine && goldMine.ConsumeFood())
                {
                    Transition((int)FSM_Villager_Flags.OnMining);
                }
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            Voronoi voronoi = parameters[0] as Voronoi;
            Villager villager = parameters[1] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += TakeRefuge;
                goldMine = voronoi.GetMineCloser(villager.Position);

                // Check when returns to take refuge state
                if (Vector2.Distance(villager.Position, goldMine.Position) > 1f) Transition((int)FSM_Villager_Flags.OnGoMine);
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
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