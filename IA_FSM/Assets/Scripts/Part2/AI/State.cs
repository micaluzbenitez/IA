using System;
using System.Collections.Generic;

namespace Part2.AI
{
    public class State
    {
        public List<Action> OnEnterBehaviours;
        public List<Action> OnBehaviours;
        public List<Action> OnExitBehaviours;
    }
}