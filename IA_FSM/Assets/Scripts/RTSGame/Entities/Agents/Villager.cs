using System;
using UnityEngine;
using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using RTSGame.Entities.Agents.VillagerStates;

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
        [SerializeField] private FSM_Caravan_States currentState;

        [Header("Movement")]
        [SerializeField] private float speed = 5f;

        [Header("Gold mine")]
        [SerializeField] private float timePerMine;
        [SerializeField] private int maxGoldRecolected;

        [Header("Food")]
        [SerializeField] private int goldsPerFood;

        [Header("UI")]
        [SerializeField] private TextMesh goldText;

        private AgentPathNodes agentPathNodes;

        private UrbanCenter urbanCenter;
        private GoldMine goldMine;

        private FSM fsm;
        private FSM_Caravan_States previousState;

        private void Awake()
        {
            agentPathNodes = GetComponent<AgentPathNodes>();
            goldText.text = "0";
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
            fsm.AddState<VillagerStates.GoingToMineState>((int)FSM_Villager_States.GoingToMine,
                () => (new object[3] { agentPathNodes, transform, speed }));

            fsm.AddState<MineState>((int)FSM_Villager_States.Mine,
                () => (new object[5] { goldMine, timePerMine, maxGoldRecolected, goldsPerFood, goldText }));

            fsm.AddState<EatState>((int)FSM_Villager_States.Eat,
                () => (new object[1] { goldMine }));

            fsm.AddState<GoingToSaveMaterialsState>((int)FSM_Villager_States.GoingToSaveMaterials,
                () => (new object[2] { transform, speed }),
                () => (new object[2] { agentPathNodes, transform }));

            fsm.AddState<SaveMaterialsState>((int)FSM_Villager_States.SaveMaterials,
                () => (new object[3] { urbanCenter, maxGoldRecolected, goldText }));

            fsm.AddState<TakeRefugeState>((int)FSM_Villager_States.TakeRefuge,
                () => (new object[3] { transform, speed, previousState }),
                () => (new object[2] { agentPathNodes, transform }));

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_Villager_States.GoingToMine);
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
            if (collision.CompareTag("UrbanCenter"))
            {
                urbanCenter = collision.GetComponent<UrbanCenter>();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine"))
            {
                goldMine = null;
            }
            if (collision.CompareTag("UrbanCenter"))
            {
                urbanCenter = null;
            }
        }
    }
}