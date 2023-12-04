using System;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Agents.States.CaravanStates;
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

        [Header("Food")]
        [SerializeField] private int foodPerTravel;

        [Header("UI")]
        [SerializeField] private TextMesh foodText;

        private FSM_Caravan_States previousState;
        private string foodQuantityText;
        private bool returnsToTakeRefuge = false;

        StateParameters allParameters;

        // Properties
        public string FoodQuantityText
        {
            get { return foodQuantityText; }
            set { foodQuantityText = value; }
        }
        public bool ReturnsToTakeRefuge
        {
            get { return returnsToTakeRefuge; }
            set { returnsToTakeRefuge = value; }
        }

        private void Awake()
        {
            foodText.text = "0";

            // Recalculate voronoi on changes
            for (int i = 0; i < MapGenerator.goldMines.Count; i++)
            {
                MapGenerator.goldMines[i].OnGoldMineBeingUsed += RecalculateVoronoi;
            }
        }

        protected override void Start()
        {
            base.Start();

            foodQuantityText = foodText.text;
            fsm = new FSM(Enum.GetValues(typeof(FSM_Caravan_States)).Length, Enum.GetValues(typeof(FSM_Caravan_Flags)).Length);

            allParameters = new StateParameters();

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

            allParameters.Parameters = new object[6] { agentPathNodes, voronoi, this, speed, foodPerTravel, previousState };
            //                                                0            1      2      3        4               5

            // Add states
            fsm.AddState<GoingToTakeFoodState>((int)FSM_Caravan_States.GoingToTakeFood, allParameters, allParameters);
            fsm.AddState<TakeFoodState>((int)FSM_Caravan_States.TakeFood, allParameters, allParameters);
            fsm.AddState<GoingToMineState>((int)FSM_Caravan_States.GoingToMine, allParameters, allParameters);
            fsm.AddState<DeliverMineState>((int)FSM_Caravan_States.DeliverFood, allParameters, allParameters);
            fsm.AddState<TakeRefugeState>((int)FSM_Caravan_States.TakeRefuge, allParameters, allParameters);

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Caravan_States.TakeFood);

            // Voronoi
            voronoi.Init();
            voronoi.SetVoronoi(MapGenerator.goldMinesBeingUsed);
        }

        protected override void Update()
        {
            base.Update();
            foodText.text = foodQuantityText;

            previousState = (FSM_Caravan_States)fsm.previousStateIndex;
            currentState = (FSM_Caravan_States)fsm.currentStateIndex;
        }

        private void RecalculateVoronoi()
        {
            voronoi.SetVoronoi(MapGenerator.goldMinesBeingUsed);
        }
    }
}