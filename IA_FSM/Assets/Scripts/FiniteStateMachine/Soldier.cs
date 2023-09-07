using System;
using UnityEngine;
using FiniteStateMachine.States;

namespace FiniteStateMachine
{
    internal enum FSM_States
    {
        Idle,
        Cut,
        SaveMaterials
    }

    internal enum FSM_Flags
    {
        OnGoWork,
        OnSaveMaterials,
        OnFinishSaveMaterials,
    }

    public class Soldier : MonoBehaviour
    {
        [SerializeField] private float speed;
        
        [Header("Cut state")]
        [SerializeField] private float cutDuration;
        [SerializeField] private Transform objetiveTransform;

        [Header("Save Materials state")]
        [SerializeField] private float saveMaterialsDuration;
        [SerializeField] private Transform materialsBoxTransform;

        private FSM fsm;

        private void Start()
        {
            fsm = new FSM(Enum.GetValues(typeof(FSM_States)).Length, Enum.GetValues(typeof(FSM_Flags)).Length);

            fsm.SetRelation((int)FSM_States.Idle, (int)FSM_Flags.OnGoWork, (int)FSM_States.Cut);
            fsm.SetRelation((int)FSM_States.Idle, (int)FSM_Flags.OnGoWork, (int)FSM_States.Cut);
            fsm.SetRelation((int)FSM_States.Cut, (int)FSM_Flags.OnSaveMaterials, (int)FSM_States.SaveMaterials);
            fsm.SetRelation((int)FSM_States.SaveMaterials, (int)FSM_Flags.OnFinishSaveMaterials, (int)FSM_States.Idle);

            fsm.AddState<IdleState>((int)FSM_States.Idle);
            fsm.AddState<CutState>((int)FSM_States.Cut,
                () => (new object[4] { gameObject.transform, objetiveTransform, speed, cutDuration }));
            fsm.AddState<SaveMaterialsState>((int)FSM_States.SaveMaterials,
                () => (new object[4] { gameObject.transform, materialsBoxTransform, speed, saveMaterialsDuration }));

            fsm.SetCurrentStateForced((int)FSM_States.Idle);
        }

        private void Update()
        {
            fsm.Update();
        }

        /*
        public void UpdateData()
        {
            ParallelOptions options = new ParallelOptions();
            
            for (int i = 0; i < soldiers.Count; i++)
            {
                List<(Vector3 minerPos, int currentState, Vector3 targetPos)> threadData = new List<(Vector3 minerPos, int currentState, Vector3 targetPos)>();
            }
            
            Parallel.ForEach(threadData, options, threadData =>
            {
                threadData.soldiers.Add(7);
            
            });
        }
        */
    }
}