using System;
using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class GoingToSaveMaterialsState : State
    {
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
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
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
                    if (villager.CurrentPathIndex >= villager.PathVectorList.Count)
                    {
                        villager.PathVectorList = null; // Stop moving
                        Transition((int)FSM_Villager_Flags.OnSaveMaterials);
                    }
                }
            }
        }
    }
}