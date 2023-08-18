using System;
using System.Collections.Generic;
using UnityEngine;
using Part2.AI.Soldier;

namespace Part2.AI
{
    public class IdleState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit raycastHit))
                    {
                        if (raycastHit.transform.gameObject.tag == "Tree")
                        {
                            Transition((int)Flags.OnGoWork);
                        }
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