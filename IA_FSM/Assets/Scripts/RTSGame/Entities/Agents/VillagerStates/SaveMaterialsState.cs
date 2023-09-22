using System.Collections.Generic;
using System;
using RTSGame.Entities.Buildings;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents.VillagerStates
{
    public class SaveMaterialsState : State
    {
        public static Action OnSaveMaterials;

        public override List<Action> GetBehaviours(params object[] parameters)
        {
            UrbanCenter urbanCenter = parameters[0] as UrbanCenter;
            int goldQuantity = Convert.ToInt32(parameters[1]);

            List<Action> behaviours = new List<Action>();
            behaviours.Add(() =>
            {
                urbanCenter.DeliverGold(goldQuantity);
                OnSaveMaterials?.Invoke();
                Transition((int)FSM_Flags.OnGoMine);
            });

            return behaviours;
        }

        public override List<Action> GetOnEnterBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override List<Action> GetExitBehaviours(params object[] parameters)
        {
            return new List<Action>();
        }

        public override void Transition(int flag)
        {
            SetFlag?.Invoke(flag);
        }
    }
}