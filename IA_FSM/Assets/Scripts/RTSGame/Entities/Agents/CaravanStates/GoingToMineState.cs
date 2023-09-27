using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.CaravanStates
{
    public class GoingToMineState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList = new List<Vector3>();

        private GoldMine goldMine;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            AgentPathNodes agentPathNodes = parameters[0] as AgentPathNodes;
            Voronoi voronoi = parameters[1] as Voronoi;
            Caravan caravan = parameters[2] as Caravan;
            float speed = Convert.ToSingle(parameters[3]);
            float deltaTime = Convert.ToSingle(parameters[4]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!goldMine) CheckForGoldMine(caravan, agentPathNodes, voronoi);
                else HandleMovement(caravan, speed, deltaTime);

                CheckActualGoldMine();
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
                goldMine = null;
                pathVectorList = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void CheckForGoldMine(Caravan caravan, AgentPathNodes agentPathNodes, Voronoi voronoi)
        {
            goldMine = voronoi.GetMineCloser(caravan.Position);

            if (goldMine)
            {
                SetTargetPosition(caravan, goldMine, agentPathNodes);
            }
        }

        protected void SetTargetPosition(Caravan caravan, GoldMine goldMine, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(caravan.Position, goldMine.Position, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        protected void HandleMovement(Caravan caravan, float speed, float deltaTime)
        {
            if (pathVectorList != null)
            {
                Vector3 targetPosition = pathVectorList[currentPathIndex];

                if (Vector3.Distance(caravan.Position, targetPosition) > 1f)
                {
                    Vector3 moveDir = (targetPosition - caravan.Position).normalized;
                    caravan.Position = caravan.Position + moveDir * speed * deltaTime;
                }
                else
                {
                    currentPathIndex++;
                    if (currentPathIndex >= pathVectorList.Count)
                    {
                        pathVectorList = null; // Stop moving
                        Transition((int)FSM_Caravan_Flags.OnDeliveringFood);
                    }
                }
            }
        }

        private void CheckActualGoldMine()
        {
            if (goldMine)
            {
                if (!goldMine.BeingUsed) goldMine = null;
            }
        }
    }
}