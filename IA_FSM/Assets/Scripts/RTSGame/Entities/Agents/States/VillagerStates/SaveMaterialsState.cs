using System;
using System.Collections.Generic;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.States.VillagerStates
{
    public class SaveMaterialsState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            Villager villager = parameters[0] as Villager;
            UrbanCenter urbanCenter = parameters[1] as UrbanCenter;
            int goldRecolected = Convert.ToInt32(parameters[2]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                urbanCenter.DeliverGold(goldRecolected);
                villager.GoldQuantityText = "0";
                Transition((int)FSM_Villager_Flags.OnGoMine);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                Alarm.OnStartAlarm += () => { Transition((int)FSM_Villager_Flags.OnTakingRefuge); };
            });

            return behaviours;
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
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