using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine
{
    public abstract class State : MonoBehaviour
    {
        public Action<int> SetFlag;
        public abstract List<Action> GetOnEnterBehaviours(params object[] parameters);
        public abstract List<Action> GetBehaviours(params object[] parameters);
        public abstract List<Action> GetExitBehaviours(params object[] parameters);
        public abstract void Transition(int flag);
    }
}