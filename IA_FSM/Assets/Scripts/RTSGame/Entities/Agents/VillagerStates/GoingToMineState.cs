using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class GoingToMineState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList;

        private GoldMine goldMine;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            AgentPathNodes agentPathNodes = parameters[0] as AgentPathNodes;
            Transform transform = parameters[1] as Transform;
            float speed = Convert.ToSingle(parameters[2]);
            Voronoi voronoi = parameters[3] as Voronoi;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                CheckForGoldMine(transform, agentPathNodes, voronoi);
                HandleMovement(transform, speed);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                goldMine = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void CheckForGoldMine(Transform transform, AgentPathNodes agentPathNodes, Voronoi voronoi)
        {
            if (goldMine) return;

            goldMine = voronoi.GetMineCloser(transform.position);

            if (goldMine)
            {
                SetTargetPosition(transform, goldMine, agentPathNodes);
            }
        }

        private void SetTargetPosition(Transform transform, GoldMine goldMine, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(transform.position, goldMine.transform.position, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Transform transform, float speed)
        {
            if (pathVectorList != null)
            {
                Vector3 targetPosition = pathVectorList[currentPathIndex];

                if (Vector3.Distance(transform.position, targetPosition) > 1f)
                {
                    Vector3 moveDir = (targetPosition - transform.position).normalized;
                    transform.position = transform.position + moveDir * speed * Time.deltaTime;
                }
                else
                {
                    currentPathIndex++;
                    if (currentPathIndex >= pathVectorList.Count)
                    {
                        goldMine.AddVillager();
                        pathVectorList = null; // Stop moving
                        Transition((int)FSM_Villager_Flags.OnMining);
                    }
                }
            }
        }
    }
}