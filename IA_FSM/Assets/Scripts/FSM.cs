using System;
using System.Collections.Generic;

public class FSM
{
    Dictionary<int, State> behaviours;
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
        behaviours = new Dictionary<int, State>();
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
            foreach (Action OnExit in behaviours[currentStateIndex].OnExitBehaviours)
                OnExit?.Invoke();

            currentStateIndex = relations[currentStateIndex, flag];

            foreach (Action OnEnter in behaviours[currentStateIndex].OnEnterBehaviours)
                OnEnter?.Invoke();
        }
    }

    public void AddBehaviour(int state, Action behaviour)
    {
        if (behaviours.ContainsKey(state))
        {
            behaviours[state].OnBehaviours.Add(behaviour);
        }
        else
        {
            State newState = new State();
            newState.OnBehaviours = new List<Action>();
            newState.OnEnterBehaviours = new List<Action>();
            newState.OnExitBehaviours = new List<Action>();
            newState.OnBehaviours.Add(behaviour);
            behaviours.Add(state, newState);
        }
    }

    public void AddOnEnterBehaviour(int state, Action behaviour) 
    {
        if (behaviours.ContainsKey(state))
        {
            behaviours[state].OnEnterBehaviours.Add(behaviour);
        }
        else
        {
            State newState = new State();
            newState.OnBehaviours = new List<Action>();
            newState.OnEnterBehaviours = new List<Action>();
            newState.OnExitBehaviours = new List<Action>();
            newState.OnEnterBehaviours.Add(behaviour);
            behaviours.Add(state, newState);
        }
    }

    public void AddOnExitBehaviour(int state, Action behaviour)
    {
        if (behaviours.ContainsKey(state))
        {
            behaviours[state].OnExitBehaviours.Add(behaviour);
        }
        else
        {
            State newState = new State();
            newState.OnBehaviours = new List<Action>();
            newState.OnEnterBehaviours = new List<Action>();
            newState.OnExitBehaviours = new List<Action>();
            newState.OnExitBehaviours.Add(behaviour);
            behaviours.Add(state, newState);
        }
    }

    public void Update()
    {
        if (behaviours.ContainsKey(currentStateIndex))
        {
            foreach (Action behaviour in behaviours[currentStateIndex].OnBehaviours)
            {
                behaviour?.Invoke();
            }
        }
    }
}
