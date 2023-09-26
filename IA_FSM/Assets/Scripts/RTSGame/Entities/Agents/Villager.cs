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
    public class Villager : MonoBehaviour
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

        [Header("Voronoi")]
        [SerializeField] private Voronoi voronoi = null;

        [Header("UI")]
        [SerializeField] private TextMesh goldText;

        [Header("Debug")]
        [SerializeField] private bool drawVoronoi;

        private AgentPathNodes agentPathNodes;

        private UrbanCenter urbanCenter;

        private FSM fsm;
        private FSM_Villager_States previousState;

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
                () => (new object[4] { agentPathNodes, transform, speed, voronoi }));

            fsm.AddState<MineState>((int)FSM_Villager_States.Mine,
                () => (new object[] { timePerMine, maxGoldRecolected, goldsPerFood, goldText }),
                () => (new object[3] { transform, voronoi, goldsPerFood }));

            fsm.AddState<EatState>((int)FSM_Villager_States.Eat,
                () => (new object[] { }),
                () => (new object[2] { transform, voronoi }));

            fsm.AddState<GoingToSaveMaterialsState>((int)FSM_Villager_States.GoingToSaveMaterials,
                () => (new object[2] { transform, speed }),
                () => (new object[3] { agentPathNodes, transform, urbanCenter }));

            fsm.AddState<SaveMaterialsState>((int)FSM_Villager_States.SaveMaterials,
                () => (new object[3] { urbanCenter, maxGoldRecolected, goldText }));

            fsm.AddState<TakeRefugeState>((int)FSM_Villager_States.TakeRefuge,
                () => (new object[3] { transform, speed, previousState }),
                () => (new object[3] { agentPathNodes, transform, urbanCenter }));

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Villager_States.GoingToMine);

            // Voronoi
            voronoi.Init();
            voronoi.SetVoronoi(MapGenerator.goldMines);
        }

        private void Update()
        {
            fsm.Update();

            previousState = (FSM_Villager_States)fsm.previousStateIndex;
            currentState = (FSM_Villager_States)fsm.currentStateIndex;
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