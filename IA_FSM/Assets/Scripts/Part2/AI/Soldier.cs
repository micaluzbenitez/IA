using Part1.AI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Part2.AI.Soldier
{
    internal enum States
    {
        Idle,
        Cut,
        SaveMaterials
    }

    internal enum Flags
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
            fsm = new FSM(Enum.GetValues(typeof(States)).Length, Enum.GetValues(typeof(Flags)).Length);

            fsm.SetRelation((int)States.Idle, (int)Flags.OnGoWork, (int)States.Cut);
            fsm.SetRelation((int)States.Idle, (int)Flags.OnGoWork, (int)States.Cut);
            fsm.SetRelation((int)States.Cut, (int)Flags.OnSaveMaterials, (int)States.SaveMaterials);
            fsm.SetRelation((int)States.SaveMaterials, (int)Flags.OnFinishSaveMaterials, (int)States.Idle);

            fsm.AddState<IdleState>((int)States.Idle);
            fsm.AddState<CutState>((int)States.Cut,
                () => (new object[4] { gameObject.transform, objetiveTransform, speed, cutDuration }));
            fsm.AddState<SaveMaterialsState>((int)States.SaveMaterials,
                () => (new object[4] { gameObject.transform, materialsBoxTransform, speed, saveMaterialsDuration }));

            fsm.SetCurrentStateForced((int)States.Idle);
        }

        private void Update()
        {
            fsm.Update();
        }

        public void UpdateData()
        {
            //ParallelOptions options = new ParallelOptions();
            //
            //for (int i = 0; i < soldiers.Count; i++)
            //{
            //    List<(Vector3 minerPos, int currentState, Vector3 targetPos)> threadData = new List<(Vector3 minerPos, int currentState, Vector3 targetPos)>();
            //}
            //
            //Parallel.ForEach(threadData, options, threadData =>
            //{
            //    threadData.soldiers.Add(7);
            //
            //});
        }
    }
}