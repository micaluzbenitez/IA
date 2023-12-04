using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Pathfinder;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class TakeRefugeState : State
    {
        private int currentPathIndex;
        private List<Vector3> pathVectorList = new List<Vector3>();

        private FSM_Caravan_States previousState;

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Caravan caravan = stateParameters.Parameters[2] as Caravan;
            float speed = Convert.ToSingle(stateParameters.Parameters[3]);
            previousState = (FSM_Caravan_States)stateParameters.Parameters[5];

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
                Alarm.OnStopAlarm += ReturnPreviousState;
                SetTargetPosition(caravan, caravan.UrbanCenter.Position, agentPathNodes);
                caravan.ReturnsToTakeRefuge = true;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStopAlarm -= ReturnPreviousState;
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
                    if (currentPathIndex >= pathVectorList.Count) pathVectorList = null; // Stop moving
                }
            }
        }

        private void ReturnPreviousState()
        {
            switch (previousState) 
            {
                case FSM_Caravan_States.GoingToTakeFood:
                    Transition((int)FSM_Caravan_Flags.OnGoTakeFood);
                    break;
                case FSM_Caravan_States.TakeFood:
                    Transition((int)FSM_Caravan_Flags.OnTakingFood);
                    break;
                case FSM_Caravan_States.GoingToMine:
                    Transition((int)FSM_Caravan_Flags.OnGoMine);
                    break;
                case FSM_Caravan_States.DeliverFood:
                    Transition((int)FSM_Caravan_Flags.OnDeliveringFood);
                    break;
            }
        }
    }
}