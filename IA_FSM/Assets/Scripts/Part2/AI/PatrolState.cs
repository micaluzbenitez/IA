using System;
using System.Collections.Generic;
using UnityEngine;
using Part2.AI.Soldier;

namespace Part2.AI
{
    public class PatrolState : State
    {
        private float time = 0;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            float duration = Convert.ToSingle(parameters[1]);
            float speed = Convert.ToSingle(parameters[2]);
            float extends = Convert.ToSingle(parameters[3]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
                //if (Mathf.Abs(transform.position.x - (initialPatrolPosition.x + patrolExtends)) > patrolExtends) speed *= -1;
                time += Time.deltaTime;

                if (time > duration)
                {
                    time = 0;
                    Transition((int)Flags.OnGoIdle);
                }
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }
    }
}