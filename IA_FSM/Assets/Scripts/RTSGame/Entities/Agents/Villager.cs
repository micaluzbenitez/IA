using System;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using RTSGame.Entities.Agents.VillagerStates;
using VoronoiDiagram;
using RTSGame.Map;

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

        [Header("Movement")]
        [SerializeField] private float speed = 5f;

        [Header("Gold mine")]
        [SerializeField] private float timePerMine;
        [SerializeField] private int maxGoldRecolected;

        [Header("Food")]
        [SerializeField] private int goldsPerFood;

        [Header("UI")]
        [SerializeField] private TextMesh goldText;

        [Header("Voronoi")]
        [SerializeField] private Voronoi voronoi = null;
        [SerializeField] private bool drawVoronoi;

        private AgentPathNodes agentPathNodes;

        private FSM fsm;
        private FSM_Villager_States previousState;

        private UrbanCenter urbanCenter;
        private Vector3 position;
        private float deltaTime;
        private string goldQuantityText;

        // Properties
        public Vector3 Position 
        { 
            get { return position; } 
            set { position = value; } 
        }
        
        public string GoldQuantityText
        {
            get { return goldQuantityText; }
            set { goldQuantityText = value; }
        }

        private void Awake()
        {
            urbanCenter = FindObjectOfType<UrbanCenter>();
            agentPathNodes = GetComponent<AgentPathNodes>();
            goldText.text = "0";

            // Recalculate voronoi on changes
            for (int i = 0; i < MapGenerator.goldMines.Count; i++)
            {
                MapGenerator.goldMines[i].OnGoldMineEmpty += RecalculateVoronoi;
            }
        }

        private void Start()
        {
            position = transform.position;
            goldQuantityText = goldText.text;

            fsm = new FSM(Enum.GetValues(typeof(FSM_Villager_States)).Length, Enum.GetValues(typeof(FSM_Villager_Flags)).Length);

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

            // Add states
            fsm.AddState<GoingToMineState>((int)FSM_Villager_States.GoingToMine,
                () => (new object[5] { agentPathNodes, voronoi, this, speed, deltaTime }));

            fsm.AddState<MineState>((int)FSM_Villager_States.Mine,
                () => (new object[5] { this, timePerMine, maxGoldRecolected, goldsPerFood, deltaTime }),
                () => (new object[3] { voronoi, this, goldsPerFood }));

            fsm.AddState<EatState>((int)FSM_Villager_States.Eat,
                () => (new object[] { }),
                () => (new object[2] { voronoi, this }));

            fsm.AddState<GoingToSaveMaterialsState>((int)FSM_Villager_States.GoingToSaveMaterials,
                () => (new object[3] { this, speed, deltaTime }),
                () => (new object[3] { agentPathNodes, this, urbanCenter }));

            fsm.AddState<SaveMaterialsState>((int)FSM_Villager_States.SaveMaterials,
                () => (new object[3] { this, urbanCenter, maxGoldRecolected }));

            fsm.AddState<TakeRefugeState>((int)FSM_Villager_States.TakeRefuge,
                () => (new object[4] { this, speed, deltaTime, previousState }),
                () => (new object[3] { agentPathNodes, this, urbanCenter }));

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Villager_States.GoingToMine);

            // Voronoi
            voronoi.Init();
            voronoi.SetVoronoi(MapGenerator.goldMines);
        }

        private void Update()
        {
            transform.position = position;
            deltaTime = Time.deltaTime;
            goldText.text = goldQuantityText;

            previousState = (FSM_Villager_States)fsm.previousStateIndex;
            currentState = (FSM_Villager_States)fsm.currentStateIndex;
        }

        public override void UpdateAgent()
        {
            fsm.Update();
        }

        private void RecalculateVoronoi(GoldMine goldMine)
        {
            MapGenerator.Instance.RemoveEmptyMine(goldMine);
            voronoi.SetVoronoi(MapGenerator.goldMines);
        }

        private void OnDrawGizmos()
        {
            if (drawVoronoi) voronoi.Draw();
        }
    }
}