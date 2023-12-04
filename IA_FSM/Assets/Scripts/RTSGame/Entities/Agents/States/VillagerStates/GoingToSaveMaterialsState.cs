using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings; 

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class GoingToSaveMaterialsState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList = new List<Vector3>();

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;
            float speed = Convert.ToSingle(stateParameters.Parameters[3]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                HandleMovement(villager, speed);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            AgentPathNodes agentPathNodes = stateParameters.Parameters[0] as AgentPathNodes;
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                SetTargetPosition(villager, villager.UrbanCenter.Position, agentPathNodes);
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
                pathVectorList = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void SetTargetPosition(Villager villager, Vector3 targetPosition, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(villager.Position, targetPosition, agentPathNodes.pathNodeWalkables);

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
                        pathVectorList = null; // Stop moving
                        Transition((int)FSM_Villager_Flags.OnSaveMaterials);
                    }
                }
            }
        }
    }
}