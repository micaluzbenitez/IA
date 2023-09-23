using System;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class SaveMaterialsState : State
    {
        public override List<Action> GetBehaviours(params object[] parameters)
        {
            UrbanCenter urbanCenter = parameters[0] as UrbanCenter;
            int goldRecolected = Convert.ToInt32(parameters[1]);
            TextMesh goldText = parameters[2] as TextMesh;

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                urbanCenter.DeliverGold(goldRecolected);
                goldText.text = "0";
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