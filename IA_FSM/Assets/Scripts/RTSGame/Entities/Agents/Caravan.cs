using System;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using RTSGame.Entities.Agents.CaravanStates;

namespace RTSGame.Entities.Agents
{
    internal enum FSM_Caravan_States
    {
        GoingToTakeFood,
        TakeFood,
        GoingToMine,
        DeliverFood,
        TakeRefuge
    }

    internal enum FSM_Caravan_Flags
    {
        OnGoTakeFood,
        OnTakingFood,
        OnGoMine,
        OnDeliveringFood,
        OnTakingRefuge
    }

    [RequireComponent(typeof(AgentPathNodes))]
    public class Caravan : MonoBehaviour
    {
        [Header("FSM")]
        [SerializeField] private FSM_Caravan_States currentState;

        [Header("Movement")]
        [SerializeField] private float speed = 5f;

        [Header("Food")]
        [SerializeField] private int foodPerTravel;

        [Header("UI")]
        [SerializeField] private TextMesh foodText;

        private AgentPathNodes agentPathNodes;

        private GoldMine goldMine;

        private FSM fsm;
        private FSM_Caravan_States previousState;

        private void Awake()
        {
            agentPathNodes = GetComponent<AgentPathNodes>();
            foodText.text = "0";
        }

        private void Start()
        {
            fsm = new FSM(Enum.GetValues(typeof(FSM_Caravan_States)).Length, Enum.GetValues(typeof(FSM_Caravan_Flags)).Length);

            // Set relations
            fsm.SetRelation((int)FSM_Caravan_States.GoingToTakeFood, (int)FSM_Caravan_Flags.OnTakingFood, (int)FSM_Caravan_States.TakeFood);
            fsm.SetRelation((int)FSM_Caravan_States.GoingToTakeFood, (int)FSM_Caravan_Flags.OnTakingRefuge, (int)FSM_Caravan_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Caravan_States.TakeFood, (int)FSM_Caravan_Flags.OnGoMine, (int)FSM_Caravan_States.GoingToMine);
            fsm.SetRelation((int)FSM_Caravan_States.TakeFood, (int)FSM_Caravan_Flags.OnTakingRefuge, (int)FSM_Caravan_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Caravan_States.GoingToMine, (int)FSM_Caravan_Flags.OnDeliveringFood, (int)FSM_Caravan_States.DeliverFood);
            fsm.SetRelation((int)FSM_Caravan_States.GoingToMine, (int)FSM_Caravan_Flags.OnTakingRefuge, (int)FSM_Caravan_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Caravan_States.DeliverFood, (int)FSM_Caravan_Flags.OnGoTakeFood, (int)FSM_Caravan_States.GoingToTakeFood);
            fsm.SetRelation((int)FSM_Caravan_States.DeliverFood, (int)FSM_Caravan_Flags.OnTakingRefuge, (int)FSM_Caravan_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnGoTakeFood, (int)FSM_Caravan_States.GoingToTakeFood);
            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnTakingFood, (int)FSM_Caravan_States.TakeFood);
            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnGoMine, (int)FSM_Caravan_States.GoingToMine);
            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnDeliveringFood, (int)FSM_Caravan_States.DeliverFood);

            // Add states
            fsm.AddState<GoingToTakeFoodState>((int)FSM_Caravan_States.GoingToTakeFood,
                () => (new object[2] { transform, speed }),
                () => (new object[2] { agentPathNodes, transform }));

            fsm.AddState<TakeFoodState>((int)FSM_Caravan_States.TakeFood,
                () => (new object[2] { foodPerTravel, foodText}));

            fsm.AddState<GoingToMineState>((int)FSM_Caravan_States.GoingToMine,
                () => (new object[3] { agentPathNodes, transform, speed }));

            fsm.AddState<DeliverMineState>((int)FSM_Caravan_States.DeliverFood,
                () => (new object[3] { goldMine, foodPerTravel, foodText }));

            fsm.AddState<TakeRefugeState>((int)FSM_Caravan_States.TakeRefuge,
                () => (new object[3] { transform, speed, previousState }),
                () => (new object[2] { agentPathNodes, transform }));

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Caravan_States.TakeFood);
        }

        private void Update()
        {
            fsm.Update();

            previousState = (FSM_Caravan_States)fsm.previousStateIndex;
            currentState = (FSM_Caravan_States)fsm.currentStateIndex;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine"))
            {
                goldMine = collision.GetComponent<GoldMine>();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine"))
            {
                goldMine = null;
            }
        }
    }
}