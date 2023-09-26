using System;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using RTSGame.Entities.Agents.CaravanStates;
using VoronoiDiagram;
using RTSGame.Map;

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
    public class Caravan : Agent
    {
        [Header("FSM")]
        [SerializeField] private FSM_Caravan_States currentState;

        [Header("Movement")]
        [SerializeField] private float speed = 5f;

        [Header("Food")]
        [SerializeField] private int foodPerTravel;

        [Header("UI")]
        [SerializeField] private TextMesh foodText;

        [Header("Voronoi")]
        [SerializeField] private Voronoi voronoi = null;
        [SerializeField] private bool drawVoronoi;

        private AgentPathNodes agentPathNodes;

        private UrbanCenter urbanCenter;

        private FSM fsm;
        private FSM_Caravan_States previousState;

        private void Awake()
        {
            urbanCenter = FindObjectOfType<UrbanCenter>();
            agentPathNodes = GetComponent<AgentPathNodes>();
            foodText.text = "0";

            // Recalculate voronoi on changes
            for (int i = 0; i < MapGenerator.goldMines.Count; i++)
            {
                MapGenerator.goldMines[i].OnGoldMineBeingUsed += RecalculateVoronoi;
            }
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
            fsm.SetRelation((int)FSM_Caravan_States.DeliverFood, (int)FSM_Caravan_Flags.OnGoMine, (int)FSM_Caravan_States.GoingToMine);
            fsm.SetRelation((int)FSM_Caravan_States.DeliverFood, (int)FSM_Caravan_Flags.OnTakingRefuge, (int)FSM_Caravan_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnGoTakeFood, (int)FSM_Caravan_States.GoingToTakeFood);
            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnTakingFood, (int)FSM_Caravan_States.TakeFood);
            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnGoMine, (int)FSM_Caravan_States.GoingToMine);
            fsm.SetRelation((int)FSM_Caravan_States.TakeRefuge, (int)FSM_Caravan_Flags.OnDeliveringFood, (int)FSM_Caravan_States.DeliverFood);

            // Add states
            fsm.AddState<GoingToTakeFoodState>((int)FSM_Caravan_States.GoingToTakeFood,
                () => (new object[2] { transform, speed }),
                () => (new object[3] { agentPathNodes, transform, urbanCenter }));

            fsm.AddState<TakeFoodState>((int)FSM_Caravan_States.TakeFood,
                () => (new object[2] { foodPerTravel, foodText}));

            fsm.AddState<GoingToMineState>((int)FSM_Caravan_States.GoingToMine,
                () => (new object[4] { agentPathNodes, transform, speed, voronoi }));

            fsm.AddState<DeliverMineState>((int)FSM_Caravan_States.DeliverFood,
                () => (new object[2] { foodPerTravel, foodText }),
                () => (new object[2] { transform, voronoi }));

            fsm.AddState<TakeRefugeState>((int)FSM_Caravan_States.TakeRefuge,
                () => (new object[3] { transform, speed, previousState }),
                () => (new object[3] { agentPathNodes, transform, urbanCenter }));

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Caravan_States.TakeFood);

            // Voronoi
            voronoi.Init();
            voronoi.SetVoronoi(MapGenerator.goldMinesBeingUsed);
        }

        private void Update()
        {
            UpdateAgent();
        }

        public override void UpdateAgent()
        {
            fsm.Update();

            previousState = (FSM_Caravan_States)fsm.previousStateIndex;
            currentState = (FSM_Caravan_States)fsm.currentStateIndex;
        }

        private void RecalculateVoronoi()
        {
            voronoi.SetVoronoi(MapGenerator.goldMinesBeingUsed);
        }

        private void OnDrawGizmos()
        {
            if (drawVoronoi) voronoi.Draw();
        }
    }
}