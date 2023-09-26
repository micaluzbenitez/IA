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
            Villager villager = parameters[0] as Villager;
            float speed = Convert.ToSingle(parameters[1]);
            float deltaTime = Convert.ToSingle(parameters[2]);
            previousState = (FSM_Villager_States)parameters[3];

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                HandleMovement(villager, speed, deltaTime);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            AgentPathNodes agentPathNodes = parameters[0] as AgentPathNodes;
            Villager villager = parameters[1] as Villager;
            UrbanCenter urbanCenter = parameters[2] as UrbanCenter;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStopAlarm += ReturnPreviousState;
                SetTargetPosition(villager, urbanCenter.transform.position, agentPathNodes);
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

        private void SetTargetPosition(Villager villager, Vector3 targetPosition, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(villager.Position, targetPosition, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Villager villager, float speed, float deltaTime)
        {
            if (pathVectorList != null)
            {
                Vector3 targetPosition = pathVectorList[currentPathIndex];

                if (Vector3.Distance(villager.Position, targetPosition) > 1f)
                {
                    Vector3 moveDir = (targetPosition - villager.Position).normalized;
                    villager.Position = villager.Position + moveDir * speed * deltaTime;
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