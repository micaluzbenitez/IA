using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Pathfinder;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class TakeRefugeState : State
    {
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
            Caravan caravan = stateParameters.Parameters[2] as Caravan;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStopAlarm -= ReturnPreviousState;
                caravan.PathVectorList = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void SetTargetPosition(Caravan caravan, Vector3 targetPosition, AgentPathNodes agentPathNodes)
        {
            caravan.CurrentPathIndex = 0;
            caravan.PathVectorList = Pathfinding.Instance.FindPath(caravan.Position, targetPosition, agentPathNodes.pathNodeWalkables);

            if (caravan.PathVectorList != null && caravan.PathVectorList.Count > 1)
            {
                caravan.PathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Caravan caravan, float speed, float deltaTime)
        {
            if (caravan.PathVectorList != null && caravan.PathVectorList.Count > 0)
            {
                Vector3 targetPosition = caravan.PathVectorList[caravan.CurrentPathIndex];
                caravan.Target = targetPosition;

                if (Vector3.Distance(caravan.Position, targetPosition) > 1f)
                {
                    Vector3 moveDir = (targetPosition - caravan.Position).normalized;
                    caravan.Position += moveDir * speed * deltaTime;
                }
                else
                {
                    caravan.CurrentPathIndex++;
                    if (caravan.CurrentPathIndex >= caravan.PathVectorList.Count) caravan.PathVectorList = null; // Stop moving
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