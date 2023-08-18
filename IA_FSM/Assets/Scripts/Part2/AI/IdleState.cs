using System;
using System.Collections.Generic;
using UnityEngine;
using Part2.AI.Soldier;

namespace Part2.AI
{
    public class IdleState : State
    {
        private float time = 0;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            float duration = Convert.ToSingle(parameters[1]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                time += Time.deltaTime;

                if (time > duration)
                {
                    time = 0;
                    //initialPatrolPosition = transform.position;
                    Transition((int)Flags.OnGoPatrol);
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