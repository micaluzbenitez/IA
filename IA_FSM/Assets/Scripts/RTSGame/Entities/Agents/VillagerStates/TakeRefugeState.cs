using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Pathfinder;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class TakeRefugeState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList;

        private FSM_Villager_States previousState;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            float speed = Convert.ToSingle(parameters[1]);
            previousState = (FSM_Villager_States)parameters[2];

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
                Alarm.OnStopAlarm += ReturnPreviousState;
                SetTargetPosition(transform, FindUrbanCenter(), agentPathNodes);
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStopAlarm -= ReturnPreviousState;
            });

            return behaviours;
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
                    if (currentPathIndex >= pathVectorList.Count) pathVectorList = null; // Stop moving
                }
            }
        }

        private void ReturnPreviousState()
        {
            switch (previousState) 
            {
                case FSM_Villager_States.GoingToMine:
                    Transition((int)FSM_Villager_Flags.OnGoMine);
                    break;
                case FSM_Villager_States.Mine:
                    Transition((int)FSM_Villager_Flags.OnMining);
                    break;
                case FSM_Villager_States.Eat:
                    Transition((int)FSM_Villager_Flags.OnGoEat);
                    break;
                case FSM_Villager_States.GoingToSaveMaterials:
                    Transition((int)FSM_Villager_Flags.OnGoSaveMaterials);
                    break;
                case FSM_Villager_States.SaveMaterials:
                    Transition((int)FSM_Villager_Flags.OnSaveMaterials);
                    break;
            }
        }
    }
}