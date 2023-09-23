using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings; 

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class GoingToSaveMaterialsState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            float speed = Convert.ToSingle(parameters[1]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                HandleMovement(transform, speed);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            AgentPathNodes agentPathNodes = parameters[0] as AgentPathNodes;
            Transform transform = parameters[1] as Transform;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                SetTargetPosition(transform, FindUrbanCenter(), agentPathNodes);
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void SetTargetPosition(Transform transform, Vector3 targetPosition, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(transform.position, targetPosition, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        private Vector3 FindUrbanCenter()
        {
            UrbanCenter urbanCenter = FindObjectOfType<UrbanCenter>();
            return urbanCenter.gameObject.transform.position;
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
                        pathVectorList = null; // Stop moving
                        Transition((int)FSM_Villager_Flags.OnSaveMaterials);
                    }
                }
            }
        }
    }
}