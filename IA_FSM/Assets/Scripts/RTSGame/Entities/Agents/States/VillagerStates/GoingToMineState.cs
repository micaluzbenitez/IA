using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class GoingToMineState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList = new List<Vector3>();

        private GoldMine goldMine;

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            AgentPathNodes agentPathNodes = stateParameters.Parameters[0] as AgentPathNodes;
            Voronoi voronoi = stateParameters.Parameters[1] as Voronoi;
            Villager villager = stateParameters.Parameters[2] as Villager;
            float speed = Convert.ToSingle(stateParameters.Parameters[3]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                CheckForGoldMine(villager, agentPathNodes, voronoi);
                HandleMovement(villager, speed);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                villager.ReturnsToTakeRefuge = false;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                goldMine = null;
                pathVectorList = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void CheckForGoldMine(Villager villager, AgentPathNodes agentPathNodes, Voronoi voronoi)
        {
            if (goldMine) return;

            goldMine = voronoi.GetMineCloser(villager.Position);

            if (goldMine)
            {
                SetTargetPosition(villager, goldMine, agentPathNodes);
            }
        }

        private void SetTargetPosition(Villager villager, GoldMine goldMine, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(villager.Position, goldMine.Position, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Villager villager, float speed)
        {
            if (pathVectorList.Count > 0 && pathVectorList != null)
            {
                Vector3 targetPosition = pathVectorList[currentPathIndex];

                if (Vector3.Distance(villager.Position, targetPosition) > 1f)
                {
                    Vector3 moveDir = (targetPosition - villager.Position).normalized;
                    villager.Position = villager.Position + moveDir * speed * villager.DeltaTime;
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