using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour, IDamageable
{
    // ----------------------- States & flags ------------------------

    enum States
    {
        Idle,
        Patrol,
        GoToTarget,
        Chase,
        Attack,
        //ReceiveDamage,
        GoAway,
        //Dead
    }

    enum Flags
    {
        OnGoIdle,
        OnGoPatrol,
        OnAlertTarget,
        OnSeeTarget,
        OnNearTarget,
        OnLostTarget,
    }

    // -------------------------- Variables --------------------------

    [Header("HP")]
    [SerializeField] private float hp;

    [Header("Damage")]
    [SerializeField] private int damage;

    [Header("Target")]
    [SerializeField] private GameObject target;
    [SerializeField] private float targetDetectionDistance;
    [SerializeField] private float targetChaseDistance;
    [SerializeField] private float targetAttackDistance;

    [Header("Idle state")]
    [SerializeField] private float idleDuration;

    [Header("Patrol state")]
    [SerializeField] private float patrolDuration;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private float patrolExtends;

    [Header("Go to target state")]
    [SerializeField] private float goToTargetSpeed;

    [Header("Chase state")]
    [SerializeField] private float chaseSpeed;

    [Header("Go away state")]
    [SerializeField] private float goAwaySpeed;

    private FSM fsm;
    private Vector3 initialPosition;
    private Vector3 initialPatrolPosition;
    private float time = 0;

    // ------------------------ Unity methods -------------------------

    private void Start()
    {
        fsm = new FSM(Enum.GetValues(typeof(States)).Length, Enum.GetValues(typeof(Flags)).Length);

        SetFSMRelations();
        AddFSMBehaviours();

        fsm.SetCurrentStateForced((int)States.Idle);
        initialPosition = transform.position;
    }

    private void Update()
    {
        fsm.Update();
    }

    // ------------------------- FSM methods --------------------------

    private void SetFSMRelations()
    {
        fsm.SetRelation((int)States.Idle, (int)Flags.OnGoPatrol, (int)States.Patrol);
        fsm.SetRelation((int)States.Idle, (int)Flags.OnAlertTarget, (int)States.GoToTarget);

        fsm.SetRelation((int)States.Patrol, (int)Flags.OnGoIdle, (int)States.Idle);
        fsm.SetRelation((int)States.Patrol, (int)Flags.OnAlertTarget, (int)States.GoToTarget);

        fsm.SetRelation((int)States.GoToTarget, (int)Flags.OnSeeTarget, (int)States.Chase);
        fsm.SetRelation((int)States.GoToTarget, (int)Flags.OnLostTarget, (int)States.GoAway);

        fsm.SetRelation((int)States.Chase, (int)Flags.OnNearTarget, (int)States.Attack);
        fsm.SetRelation((int)States.Chase, (int)Flags.OnLostTarget, (int)States.GoAway);

        fsm.SetRelation((int)States.Attack, (int)Flags.OnLostTarget, (int)States.GoAway);

        fsm.SetRelation((int)States.GoAway, (int)Flags.OnGoIdle, (int)States.Idle);
        fsm.SetRelation((int)States.GoAway, (int)Flags.OnAlertTarget, (int)States.GoToTarget);
    }

    private void AddFSMBehaviours()
    {
        fsm.AddBehaviour((int)States.Idle, () => Debug.Log("Idle"));
        fsm.AddBehaviour((int)States.Idle, () =>
        {
            time += Time.deltaTime;

            if (time > idleDuration)
            {
                time = 0;
                initialPatrolPosition = transform.position;
                fsm.SetFlag((int)Flags.OnGoPatrol);
            }
            if (Vector3.Distance(transform.position, target.transform.position) < targetDetectionDistance)
            {
                time = 0;
                fsm.SetFlag((int)Flags.OnAlertTarget);
            }
        });

        fsm.AddBehaviour((int)States.Patrol, () => Debug.Log("Patrol"));
        fsm.AddBehaviour((int)States.Patrol, () =>
        {
            transform.position += Vector3.right * patrolSpeed * Time.deltaTime;
            if (Mathf.Abs(transform.position.x - (initialPatrolPosition.x + patrolExtends)) > patrolExtends) patrolSpeed *= -1;

            time += Time.deltaTime;

            if (time > patrolDuration)
            {
                time = 0;
                fsm.SetFlag((int)Flags.OnGoIdle);
            }
            if (Vector3.Distance(transform.position, target.transform.position) < targetDetectionDistance)
            {
                time = 0;
                fsm.SetFlag((int)Flags.OnAlertTarget);
            }
        });


        fsm.AddBehaviour((int)States.GoToTarget, () => Debug.Log("Go to target"));
        fsm.AddBehaviour((int)States.GoToTarget, () =>
        {
            transform.position += (target.transform.position - transform.position).normalized * goToTargetSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, target.transform.position) < targetChaseDistance)
            {
                fsm.SetFlag((int)Flags.OnSeeTarget);
            }
            if (Vector3.Distance(transform.position, target.transform.position) > targetDetectionDistance)
            {
                fsm.SetFlag((int)Flags.OnLostTarget);
            }
        });


        fsm.AddBehaviour((int)States.Chase, () => Debug.Log("Chase"));
        fsm.AddBehaviour((int)States.Chase, () =>
        {
            transform.position += (target.transform.position - transform.position).normalized * chaseSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, target.transform.position) < targetAttackDistance)
            {
                fsm.SetFlag((int)Flags.OnNearTarget);
            }
            if (Vector3.Distance(transform.position, target.transform.position) > targetChaseDistance)
            {
                fsm.SetFlag((int)Flags.OnLostTarget);
            }
        });


        fsm.AddBehaviour((int)States.Attack, () => Debug.Log("Attack"));
        fsm.AddBehaviour((int)States.Attack, () =>
        {
            if (Vector3.Distance(transform.position, target.transform.position) > targetAttackDistance)
            {
                fsm.SetFlag((int)Flags.OnLostTarget);
            }
        });


        fsm.AddBehaviour((int)States.GoAway, () => Debug.Log("Go away"));
        fsm.AddBehaviour((int)States.GoAway, () =>
        {
            transform.position += (initialPosition - transform.position).normalized * goAwaySpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, initialPosition) < 0.1f)
            {
                fsm.SetFlag((int)Flags.OnGoIdle);
            }
            if (Vector3.Distance(transform.position, target.transform.position) < targetDetectionDistance)
            {
                fsm.SetFlag((int)Flags.OnAlertTarget);
            }
        });
    }

    // ------------------------ Others methods ------------------------

    public void TakeDamage(float damage)
    {
        if (hp <= 0) return;

        hp -= damage;
        if (hp < 0) hp = 0;
    }
}
