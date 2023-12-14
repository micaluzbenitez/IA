using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Pathfinder;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class TakeRefugeState : State
    {
        private FSM_Villager_States previousState;

        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;
            float speed = Convert.ToSingle(stateParameters.Parameters[3]);
            previousState = (FSM_Villager_States)stateParameters.Parameters[7];

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
                Alarm.OnStopAlarm += ReturnPreviousState;
                SetTargetPosition(villager, villager.UrbanCenter.Position, agentPathNodes);
                villager.ReturnsToTakeRefuge = true;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStopAlarm -= ReturnPreviousState;
                villager.PathVectorList = null;
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }

        private void SetTargetPosition(Villager villager, Vector3 targetPosition, AgentPathNodes agentPathNodes)
        {
            villager.CurrentPathIndex = 0;
            villager.PathVectorList = Pathfinding.Instance.FindPath(villager.Position, targetPosition, agentPathNodes.pathNodeWalkables);

            if (villager.PathVectorList != null && villager.PathVectorList.Count > 1)
            {
                villager.PathVectorList.RemoveAt(0);
            }
        }

        private void HandleMovement(Villager villager, float speed)
        {
            if (villager.PathVectorList != null && villager.PathVectorList.Count > 0)
            {
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
                    if (villager.CurrentPathIndex >= villager.PathVectorList.Count) villager.PathVectorList = null; // Stop moving
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