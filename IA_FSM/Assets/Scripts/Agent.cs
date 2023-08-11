using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{ 
    /*
     * Ir a un target
     * Patrol
     * Idle
     * Atack
     * Morir
     * recivir daño
     * retirarse
     * chase
     */
    enum States
    {
        Patrol,
        Chase,
        Explode,
        Dead
    }

    enum Flags
    {
        OnSeeTarget,
        OnNearTarget,
        OnLostTarget,
        OnExplodeSuccess
    }

    public GameObject Target;

    private float speed = 5;

    private FSM fsm;

    private void Start()
    {
        fsm = new FSM(Enum.GetValues(typeof(States)).Length, Enum.GetValues(typeof(Flags)).Length);

        fsm.SetRelation((int)States.Patrol, (int)Flags.OnSeeTarget, (int)States.Chase);
        fsm.SetRelation((int)States.Patrol, (int)Flags.OnNearTarget, (int)States.Explode);

        fsm.SetRelation((int)States.Chase, (int)Flags.OnLostTarget, (int)States.Patrol);
        fsm.SetRelation((int)States.Chase, (int)Flags.OnNearTarget, (int)States.Explode);

        fsm.SetRelation((int)States.Explode, (int)Flags.OnExplodeSuccess, (int)States.Dead);

        fsm.AddBehaviour((int)States.Patrol, () =>
        {
            transform.position += Vector3.right * Time.deltaTime * speed;
            if (Mathf.Abs(transform.position.x) > 10.0f)
                speed *= -1;

            if (Vector3.Distance(transform.position, Target.transform.position) < 3.0f)
            {
                speed = Mathf.Abs(speed);
                fsm.SetFlag((int)Flags.OnSeeTarget);
            }
            if (Vector3.Distance(transform.position, Target.transform.position) < 1.0f)
            {
                speed = Mathf.Abs(speed);
                fsm.SetFlag((int)Flags.OnNearTarget);
            }
        });
        fsm.AddBehaviour((int)States.Patrol, () => Debug.Log("PATROL"));


        fsm.AddBehaviour((int)States.Chase, () =>
        {
            transform.position += (Target.transform.position - transform.position).normalized * speed * Time.deltaTime;
            if (Vector3.Distance(transform.position, Target.transform.position) < 1.0f)
            {
                fsm.SetFlag((int)Flags.OnNearTarget);
            }
            if (Vector3.Distance(transform.position, Target.transform.position) > 5.0f)
            {
                fsm.SetFlag((int)Flags.OnLostTarget);
            }
        });
        fsm.AddBehaviour((int)States.Chase, () => Debug.Log("CHASE"));


        fsm.AddBehaviour((int)States.Explode, () =>
        {
            Debug.Log("BOOM");
            fsm.SetFlag((int)Flags.OnExplodeSuccess);
        });

        fsm.AddOnEnterBehaviour((int)States.Explode, () => { Debug.Log("Adios mundo cruel"); });

        fsm.AddBehaviour((int)States.Dead, () => 
        {
            Debug.Log("F");
        });

        fsm.SetCurrentStateForced((int)States.Patrol);
    }

    private void Update()
    {
        fsm.Update();
    }
}
