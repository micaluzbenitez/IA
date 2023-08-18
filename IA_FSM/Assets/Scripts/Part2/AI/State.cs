using System;
using System.Collections.Generic;

namespace Part2.AI
{
    public abstract class State
    {
        public Action<int> SetFlag;
        public abstract List<Action> GetOnEnterBehaviours(params object[] parameters);
        public abstract List<Action> GetBehaviours(params object[] parameters);
        public abstract List<Action> GetExitBehaviours(params object[] parameters);
        public abstract void Transition(int flag);
    }
}