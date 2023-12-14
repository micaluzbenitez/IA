using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.States.CaravanStates
{
    public class GoingToTakeFoodState : State
    {
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
            Caravan caravan = stateParameters.Parameters[2] as Caravan;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Caravan_Flags.OnTakingRefuge); };
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
                    if (caravan.CurrentPathIndex >= caravan.PathVectorList.Count)
                    {
                        caravan.PathVectorList = null; // Stop moving
                        Transition((int)FSM_Caravan_Flags.OnTakingFood);
                    }
                }
            }
        }
    }
}