using UnityEngine;
using Toolbox;
using RTSGame.Entities.Buildings;
using UnityEngine.Assertions.Must;
using System.Collections.Generic;

namespace RTSGame.Entities.Agents
{
    public class Caravan : Agent
    {
        [Header("Food")]
        [SerializeField] private int foodPerTravel;

        [Header("UI")]
        [SerializeField] private TextMesh foodText;

        // Food
        private int foodQuantity;

        // Gold mine
        private bool waitingGoldMine = false;
        private GoldMine actualGoldMine;

        protected override void Awake()
        {
            base.Awake();
            foodQuantity = foodPerTravel;
            foodText.text = foodQuantity.ToString();
            waitingGoldMine = true;
        }

        protected override void Update()
        {
            base.Update();
            CheckForGoldMine();
            CheckActualGoldMine();
        }

        protected Vector3 FindNearestGoldMineBeingUsed()
        {
            GoldMine[] goldMines = FindObjectsOfType<GoldMine>();
            List<GoldMine> goldMinesBeingUsed = new List<GoldMine>();

            for (int i = 0; i < goldMines.Length; i++)
            {
                if (goldMines[i].WithVillagers) goldMinesBeingUsed.Add(goldMines[i]);
            }

            if (goldMinesBeingUsed.Count > 0)
            {
                int randomIndex = Random.Range(0, goldMinesBeingUsed.Count);
                actualGoldMine = goldMinesBeingUsed[randomIndex];
                return goldMinesBeingUsed[randomIndex].gameObject.transform.position;
            }
            else
            {
                return default;
            }
        }

        private void CheckForGoldMine()
        {
            if (waitingGoldMine)
            {
                Vector3 target = FindNearestGoldMineBeingUsed();

                if (target != default)
                {
                    SetTargetPosition(FindNearestGoldMineBeingUsed());
                    waitingGoldMine = false;
                }
                else
                {
                    return;
                }
            }
        }

        private void CheckActualGoldMine()
        {
            if (actualGoldMine)
            {
                if (!actualGoldMine.WithVillagers) waitingGoldMine = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine"))
            {
                GoldMine goldMine = collision.GetComponent<GoldMine>();
                if (goldMine.WithVillagers)
                {
                    goldMine.DeliverFood(foodQuantity);
                    foodQuantity = 0;
                    foodText.text = foodQuantity.ToString();
                    SetTargetPosition(FindUrbanCenter());
                }
            }

            if (collision.CompareTag("UrbanCenter"))
            {
                UrbanCenter urbanCenter = collision.GetComponent<UrbanCenter>();
                foodQuantity = foodPerTravel;
                foodText.text = foodQuantity.ToString();
                waitingGoldMine = true;
            }
        }
    }
}