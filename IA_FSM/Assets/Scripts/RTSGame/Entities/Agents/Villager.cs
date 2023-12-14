using System;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Agents.States.VillagerStates;
using RTSGame.Map;
using System.Collections.Generic;
using RTSGame.Entities.Buildings;

namespace RTSGame.Entities.Agents
{
    internal enum FSM_Villager_States
    {
        GoingToMine,
        Mine,
        Eat,
        GoingToSaveMaterials,
        SaveMaterials,
        TakeRefuge
    }

    internal enum FSM_Villager_Flags
    {
        OnGoMine,
        OnMining,
        OnGoEat,
        OnGoSaveMaterials,
        OnSaveMaterials,
        OnTakingRefuge
    }

    [RequireComponent(typeof(AgentPathNodes))]
    public class Villager : Agent
    {
        [Header("FSM")]
        [SerializeField] private FSM_Villager_States currentState;

        [Header("Gold mine")]
        [SerializeField] private float timePerMine;
        [SerializeField] private int maxGoldRecolected;

        [Header("Food")]
        [SerializeField] private int goldsPerFood;

        [Header("UI")]
        [SerializeField] private TextMesh goldText;

        private FSM_Villager_States previousState;
        private string goldQuantityText;
        private bool needsFood = false;
        private bool returnsToTakeRefuge = false;
        private int currentPathIndex;
        private List<Vector3> pathVectorList = new List<Vector3>();
        private GoldMine goldMine;

        StateParameters allParameters;

        // Properties
        public string GoldQuantityText
        {
            get { return goldQuantityText; }
            set { goldQuantityText = value; }
        }
        public bool NeedsFood
        {
            get { return needsFood; }
            set { needsFood = value; }
        }
        public bool ReturnsToTakeRefuge
        {
            get { return returnsToTakeRefuge; }
            set { returnsToTakeRefuge = value; }
        }
        public int CurrentPathIndex
        {
            get { return currentPathIndex; }
            set { currentPathIndex = value; }
        }
        public List<Vector3> PathVectorList
        {
            get { return pathVectorList; }
            set { pathVectorList = value; }
        }
        public GoldMine GoldMine
        {
            get { return goldMine; }
            set { goldMine = value; }
        }

        private void Awake()
        {
            goldText.text = "0";

            // Recalculate voronoi on changes
            for (int i = 0; i < MapGenerator.goldMines.Count; i++)
            {
                MapGenerator.goldMines[i].OnGoldMineEmpty += RecalculateVoronoi;
            }
        }

        protected override void Start()
        {
            base.Start();

            goldQuantityText = goldText.text;
            fsm = new FSM(Enum.GetValues(typeof(FSM_Villager_States)).Length, Enum.GetValues(typeof(FSM_Villager_Flags)).Length);

            allParameters = new StateParameters();

            // Set relations
            fsm.SetRelation((int)FSM_Villager_States.GoingToMine, (int)FSM_Villager_Flags.OnMining, (int)FSM_Villager_States.Mine);
            fsm.SetRelation((int)FSM_Villager_States.GoingToMine, (int)FSM_Villager_Flags.OnTakingRefuge, (int)FSM_Villager_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Villager_States.Mine, (int)FSM_Villager_Flags.OnGoEat, (int)FSM_Villager_States.Eat);
            fsm.SetRelation((int)FSM_Villager_States.Mine, (int)FSM_Villager_Flags.OnGoMine, (int)FSM_Villager_States.GoingToMine);
            fsm.SetRelation((int)FSM_Villager_States.Mine, (int)FSM_Villager_Flags.OnGoSaveMaterials, (int)FSM_Villager_States.GoingToSaveMaterials);
            fsm.SetRelation((int)FSM_Villager_States.Mine, (int)FSM_Villager_Flags.OnTakingRefuge, (int)FSM_Villager_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Villager_States.Eat, (int)FSM_Villager_Flags.OnMining, (int)FSM_Villager_States.Mine);
            fsm.SetRelation((int)FSM_Villager_States.Eat, (int)FSM_Villager_Flags.OnGoMine, (int)FSM_Villager_States.GoingToMine);
            fsm.SetRelation((int)FSM_Villager_States.Eat, (int)FSM_Villager_Flags.OnTakingRefuge, (int)FSM_Villager_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Villager_States.GoingToSaveMaterials, (int)FSM_Villager_Flags.OnSaveMaterials, (int)FSM_Villager_States.SaveMaterials);
            fsm.SetRelation((int)FSM_Villager_States.GoingToSaveMaterials, (int)FSM_Villager_Flags.OnTakingRefuge, (int)FSM_Villager_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Villager_States.SaveMaterials, (int)FSM_Villager_Flags.OnGoMine, (int)FSM_Villager_States.GoingToMine);
            fsm.SetRelation((int)FSM_Villager_States.SaveMaterials, (int)FSM_Villager_Flags.OnGoEat, (int)FSM_Villager_States.Eat);
            fsm.SetRelation((int)FSM_Villager_States.SaveMaterials, (int)FSM_Villager_Flags.OnTakingRefuge, (int)FSM_Villager_States.TakeRefuge);

            fsm.SetRelation((int)FSM_Villager_States.TakeRefuge, (int)FSM_Villager_Flags.OnGoMine, (int)FSM_Villager_States.GoingToMine);
            fsm.SetRelation((int)FSM_Villager_States.TakeRefuge, (int)FSM_Villager_Flags.OnMining, (int)FSM_Villager_States.Mine);
            fsm.SetRelation((int)FSM_Villager_States.TakeRefuge, (int)FSM_Villager_Flags.OnGoEat, (int)FSM_Villager_States.Eat);
            fsm.SetRelation((int)FSM_Villager_States.TakeRefuge, (int)FSM_Villager_Flags.OnGoSaveMaterials, (int)FSM_Villager_States.GoingToSaveMaterials);
            fsm.SetRelation((int)FSM_Villager_States.TakeRefuge, (int)FSM_Villager_Flags.OnSaveMaterials, (int)FSM_Villager_States.SaveMaterials);

            allParameters.Parameters = new object[8] { agentPathNodes, voronoi, this, speed, timePerMine, maxGoldRecolected, goldsPerFood, previousState };
            //                                               0            1      2      3         4               5                6             7

            // Add states
            fsm.AddState<GoingToMineState>((int)FSM_Villager_States.GoingToMine, allParameters, allParameters, allParameters);
            fsm.AddState<MineState>((int)FSM_Villager_States.Mine, allParameters, allParameters, allParameters);
            fsm.AddState<EatState>((int)FSM_Villager_States.Eat, allParameters, allParameters, allParameters);
            fsm.AddState<GoingToSaveMaterialsState>((int)FSM_Villager_States.GoingToSaveMaterials, allParameters, allParameters, allParameters);
            fsm.AddState<SaveMaterialsState>((int)FSM_Villager_States.SaveMaterials, allParameters, allParameters, allParameters);
            fsm.AddState<TakeRefugeState>((int)FSM_Villager_States.TakeRefuge, allParameters, allParameters, allParameters);

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Villager_States.GoingToMine);

            // Voronoi
            voronoi.Init();
            voronoi.SetVoronoi(MapGenerator.goldMines);
        }

        protected override void Update()
        {
            base.Update();
            goldText.text = goldQuantityText;

            previousState = (FSM_Villager_States)fsm.previousStateIndex;
            currentState = (FSM_Villager_States)fsm.currentStateIndex;
        }

        private void RecalculateVoronoi()
        {
            voronoi.SetVoronoi(MapGenerator.goldMines);
        }
    }
}