using System;
using System.Collections.Generic;

public class State
{
    public List<Action> OnEnterBehaviours;
    public List<Action> behaviours;
    public List<Action> OnExitBehaviours;
}
