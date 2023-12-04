using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class GoingToTakeFoodState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList = new List<Vector3>();

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Caravan caravan = stateParameters.Parameters[2] as Caravan;
            float speed = Convert.ToSingle(stateParameters.Parameters[3]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                HandleMovement(caravan, speed, caravan.DeltaTime);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            AgentPathNodes agentPathNodes = stateParameters.Parameters[0] as AgentPathNodes;
            Caravan caravan = stateParameters.Parameters[2] as Caravan;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
                SetTargetPosition(caravan, caravan.UrbanCenter.Position, agentPathNodes);
                caravan.ReturnsToTakeRefuge = false;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
                pathVectorList = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void SetTargetPosition(Caravan caravan, Vector3 targetPosition, AgentPathNodes agentPathNodes)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(caravan.Position, targetPosition, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Caravan caravan, float speed, float deltaTime)
        {
            if (pathVectorList.Count > 0 && pathVectorList != null)
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
                        Transition((int)FSM_Caravan_Flags.OnTakingFood);
                    }
                }
            }
        }
    }
}