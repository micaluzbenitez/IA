using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine.States
{
    public class SaveMaterialsState : State
    {
        private float time;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Transform transform = parameters[0] as Transform;
            Transform objetiveTransform = parameters[1] as Transform;
            float speed = Convert.ToSingle(parameters[2]);
            float duration = Convert.ToSingle(parameters[3]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                transform.position += (objetiveTransform.position - transform.position).normalized * speed * Time.deltaTime;

                if (Vector3.Distance(transform.position, objetiveTransform.position) < 1f)
                {
                    time += Time.deltaTime;

                    if (time > duration)
                    {
                        time = 0;
                        Transition((int)FSM_Flags.OnFinishSaveMaterials);
                    }
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