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
                goldMine = null;
                villager.PathVectorList = null;
                villager.CurrentPathIndex = 0;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                goldMine = null;
                villager.PathVectorList = null;
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
            villager.CurrentPathIndex = 0;
            villager.PathVectorList = Pathfinding.Instance.FindPath(villager.Position, goldMine.Position, agentPathNodes.pathNodeWalkables);

            if (villager.PathVectorList != null && villager.PathVectorList.Count > 1)
            {
                villager.PathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Villager villager, float speed)
        {
            if (!goldMine.WithGold)
            {
                goldMine = null;
                villager.PathVectorList = null;
                return;
            }

            if (villager.PathVectorList != null && villager.PathVectorList.Count > 0)
            {
                if (villager.CurrentPathIndex >= villager.PathVectorList.Count) return;

                Vector3 targetPosition = villager.PathVectorList[villager.CurrentPathIndex];
                villager.Target = targetPosition;

                if (Vector3.Distance(villager.Position, targetPosition) > 1f)
                {
                    Vector3 moveDir = (targetPosition - villager.Position).normalized;
                    villager.Position += moveDir * speed * villager.DeltaTime;
                }
                else
                {
                    villager.CurrentPathIndex++;
                    if (villager.CurrentPathIndex >= villager.PathVectorList.Count)
                    {
                        goldMine.AddVillager(villager);
                        villager.PathVectorList = null; // Stop moving
                        Transition((int)FSM_Villager_Flags.OnMining);
                    }
                }
            }
        }
    }
}