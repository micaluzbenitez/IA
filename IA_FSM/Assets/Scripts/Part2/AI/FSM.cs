using System;
using System.Collections.Generic;

namespace Part2.AI
{
    public class FSM
    {
        Dictionary<int, State> states;
        Dictionary<int, Func<object[]>> statesParameters;
        Dictionary<int, Func<object[]>> statesOnEnterParameters;
        Dictionary<int, Func<object[]>> statesOnExitParameters;
        public int currentStateIndex = 0;
        private int[,] relations;

        public FSM(int states, int flags)
        {
            currentStateIndex = -1;
            relations = new int[states, flags];
            for (int i = 0; i < states; i++)
            {
                for (int j = 0; j < flags; j++)
                {
                    relations[i, j] = -1;
                }
            }
            this.states = new Dictionary<int, State>();
            this.statesParameters = new Dictionary<int, Func<object[]>>();
            this.statesOnEnterParameters = new Dictionary<int, Func<object[]>>();
            this.statesOnExitParameters = new Dictionary<int, Func<object[]>>();
        }

        public void SetCurrentStateForced(int state)
        {
            currentStateIndex = state;
        }

        public void SetRelation(int sourceState, int flag, int destinationState)
        {
            relations[sourceState, flag] = destinationState;
        }

        public void SetFlag(int flag)
        {
            if (relations[currentStateIndex, flag] != -1)
            {
                foreach (Action OnExit in states[currentStateIndex].GetExitBehaviours())
                    OnExit?.Invoke();

                currentStateIndex = relations[currentStateIndex, flag];

                foreach (Action OnEnter in states[currentStateIndex].GetOnEnterBehaviours())
                    OnEnter?.Invoke();
            }
        }

        public void AddState<T>(int stateIndex, Func<object[]> stateParams = null,
            Func<object[]> stateOnEnterParams = null, Func<object[]> stateOnExitParams = null) where T : State, new()
        {
            if (!states.ContainsKey(stateIndex))
            {
                State newState = new T();
                newState.SetFlag += SetFlag;
                states.Add(stateIndex, newState);
                statesParameters.Add(stateIndex, stateParams);
                statesOnEnterParameters.Add(stateIndex, stateOnEnterParams);
                statesOnExitParameters.Add(stateIndex, stateOnExitParams);   
            }
        }

        public void Update()
        {
            if (states.ContainsKey(currentStateIndex))
            {
                foreach (Action behaviour in states[currentStateIndex].GetBehaviours(statesParameters[currentStateIndex]?.Invoke()))
                {
                    behaviour?.Invoke();
                }
            }
        }
    }
}