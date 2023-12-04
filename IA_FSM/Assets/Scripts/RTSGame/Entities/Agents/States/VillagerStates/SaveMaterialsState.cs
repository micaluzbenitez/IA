using System;
using System.Collections.Generic;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class SaveMaterialsState : State
    {
        public override List<Action> GetBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;
            int maxGoldRecolected = Convert.ToInt32(stateParameters.Parameters[5]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                villager.UrbanCenter.DeliverGold(maxGoldRecolected);
                villager.GoldQuantityText = "0";
                Transition((int)FSM_Villager_Flags.OnGoMine);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(StateParameters stateParameters)
        {
            Villager villager = stateParameters.Parameters[2] as Villager;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
                villager.ReturnsToTakeRefuge = false;
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(StateParameters stateParameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm -= () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
            });

            return behaviours;
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }
    }
}