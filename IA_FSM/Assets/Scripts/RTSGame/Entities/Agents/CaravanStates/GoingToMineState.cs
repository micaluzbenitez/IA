using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using Random = UnityEngine.Random;

namespace RTSGame.Entities.Agents.CaravanStates
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

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (!goldMine) CheckForGoldMine(transform, agentPathNodes);
                else HandleMovement(transform, speed);

                CheckActualGoldMine();
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                goldMine = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void CheckForGoldMine(Transform transform, AgentPathNodes agentPathNodes)
        {
            goldMine = FindNearestGoldMineBeingUsed();

            if (goldMine)
            {
                SetTargetPosition(transform, goldMine, agentPathNodes);
            }
        }

        protected void SetTargetPosition(Transform transform, GoldMine goldMine, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(transform.position, goldMine.transform.position, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        private GoldMine FindNearestGoldMineBeingUsed()
        {
            GoldMine[] goldMines = FindObjectsOfType<GoldMine>();
            List<GoldMine> goldMinesBeingUsed = new List<GoldMine>();

            for (int i = 0; i < goldMines.Length; i++)
            {
                if (goldMines[i].WithVillagers) goldMinesBeingUsed.Add(goldMines[i]);
            }

            if (goldMinesBeingUsed.Count > 0)
            {
                int randomIndex = Random.Range(0, goldMinesBeingUsed.Count);
                return goldMinesBeingUsed[randomIndex];
            }
            else
            {
                return default;
            }
        }

        protected void HandleMovement(Transform transform, float speed)
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
                        Transition((int)FSM_Caravan_Flags.OnDeliveringFood);
                    }
                }
            }
        }

        private void CheckActualGoldMine()
        {
            if (goldMine)
            {
                if (!goldMine.WithVillagers) goldMine = null;
            }
        }
    }
}