using System;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    public abstract class State
    {
        public Action<int> SetFlag;
        public abstract List<Action> GetOnEnterBehaviours(StateParameters stateParameters);
        public abstract List<Action> GetBehaviours(StateParameters stateParameters);
        public abstract List<Action> GetExitBehaviours(StateParameters stateParameters);
        public abstract void Transition(int flag);
    }
}