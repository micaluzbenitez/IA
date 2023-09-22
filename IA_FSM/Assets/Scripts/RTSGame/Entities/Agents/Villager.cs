using System.Collections.Generic;
using System;
using UnityEngine;
using RTSGame.Entities.Buildings;
using RTSGame.Entities.Agents.VillagerStates;
using FiniteStateMachine;

namespace RTSGame.Entities.Agents
{
    internal enum FSM_States
    {
        GoingToMine,
        Mine,
        Eat,
        GoingToSaveMaterials,
        SaveMaterials
    }

    internal enum FSM_Flags
    {
        OnGoMine,
        OnMining,
        OnGoEat,
        OnGoSaveMaterials,
        OnSaveMaterials
    }

    [RequireComponent(typeof(AgentPathNodes))]
    public class Villager : MonoBehaviour
    {
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
        private int currentPathIndex;
        private List<Vector3> pathVectorList;

        // Urban center
        private UrbanCenter urbanCenter;

        // Gold mine
        private GoldMine goldMine;
        private int goldQuantity;

        private FSM fsm;

        private void Awake()
        {
            agentPathNodes = GetComponent<AgentPathNodes>();
            goldText.text = goldQuantity.ToString();
            MineState.OnMine += AddGold;
            SaveMaterialsState.OnSaveMaterials += EmptyGold;
        }

        private void Start()
        {
            fsm = new FSM(Enum.GetValues(typeof(FSM_States)).Length, Enum.GetValues(typeof(FSM_Flags)).Length);

            // Set relations
            fsm.SetRelation((int)FSM_States.GoingToMine, (int)FSM_Flags.OnMining, (int)FSM_States.Mine);

            fsm.SetRelation((int)FSM_States.Mine, (int)FSM_Flags.OnGoEat, (int)FSM_States.Eat);
            fsm.SetRelation((int)FSM_States.Mine, (int)FSM_Flags.OnGoMine, (int)FSM_States.GoingToMine);
            fsm.SetRelation((int)FSM_States.Mine, (int)FSM_Flags.OnGoSaveMaterials, (int)FSM_States.GoingToSaveMaterials);

            fsm.SetRelation((int)FSM_States.Eat, (int)FSM_Flags.OnMining, (int)FSM_States.Mine);

            fsm.SetRelation((int)FSM_States.GoingToSaveMaterials, (int)FSM_Flags.OnSaveMaterials, (int)FSM_States.SaveMaterials);

            fsm.SetRelation((int)FSM_States.SaveMaterials, (int)FSM_Flags.OnGoMine, (int)FSM_States.GoingToMine);
            fsm.SetRelation((int)FSM_States.SaveMaterials, (int)FSM_Flags.OnGoEat, (int)FSM_States.Eat);

            // Add states
            fsm.AddState<GoingToMineState>((int)FSM_States.GoingToMine,
                () => (new object[3] { agentPathNodes, transform, speed }),
                () => (new object[2] { agentPathNodes, transform }));

            fsm.AddState<MineState>((int)FSM_States.Mine,
                () => (new object[5] { goldMine, timePerMine, goldQuantity, maxGoldRecolected, goldsPerFood }));

            fsm.AddState<EatState>((int)FSM_States.Eat,
                () => (new object[1] { goldMine }));

            fsm.AddState<GoingToSaveMaterialsState>((int)FSM_States.GoingToSaveMaterials,
                () => (new object[2] { transform, speed }),
                () => (new object[2] { agentPathNodes, transform }));

            fsm.AddState<SaveMaterialsState>((int)FSM_States.SaveMaterials,
                () => (new object[2] { urbanCenter, goldQuantity }));

            // Start FSM
            fsm.SetCurrentStateForced((int)FSM_States.GoingToMine);
        }

        private void Update()
        {
            fsm.Update();
        }

        private void AddGold()
        {
            goldQuantity++;
            goldText.text = goldQuantity.ToString();
        }

        private void EmptyGold()
        {
            goldQuantity = 0;
            goldText.text = goldQuantity.ToString();
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