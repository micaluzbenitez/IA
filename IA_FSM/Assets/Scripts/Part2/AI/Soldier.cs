using System;
using UnityEngine;

namespace Part2.AI.Soldier
{
    internal enum States
    {
        Idle,
        Patrol,
    }

    internal enum Flags
    {
        OnGoIdle,
        OnGoPatrol,
    }

    public class Soldier : MonoBehaviour
    {
        [Header("Idle state")]
        [SerializeField] private float idleDuration;

        [Header("Patrol state")]
        [SerializeField] private float patrolDuration;
        [SerializeField] private float patrolSpeed;
        [SerializeField] private float patrolExtends;

        private FSM fsm;
        private Vector3 initialPatrolPosition;

        private void Start()
        {
            fsm = new FSM(Enum.GetValues(typeof(States)).Length, Enum.GetValues(typeof(Flags)).Length);

            fsm.SetRelation((int)States.Idle, (int)Flags.OnGoPatrol, (int)States.Patrol);
            fsm.SetRelation((int)States.Patrol, (int)Flags.OnGoIdle, (int)States.Idle);

            fsm.AddState<IdleState>((int)States.Idle,
                () => (new object[2] { gameObject.transform, idleDuration }));
            fsm.AddState<PatrolState>((int)States.Patrol,
                () => (new object[4] { gameObject.transform, patrolDuration, patrolSpeed, patrolExtends }));

            fsm.SetCurrentStateForced((int)States.Idle);
        }

        private void Update()
        {
            fsm.Update();
        }
    }
}