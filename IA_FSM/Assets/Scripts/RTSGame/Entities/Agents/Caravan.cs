using UnityEngine;
using Toolbox;
using RTSGame.Entities.Buildings;

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

        protected override void Awake()
        {
            base.Awake();
            foodQuantity = foodPerTravel;
            foodText.text = foodQuantity.ToString();
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.M)) SetTargetPosition(FindNearestGoldMine());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("GoldMine"))
            {
                GoldMine goldMine = collision.GetComponent<GoldMine>();
                goldMine.DeliverFood(foodQuantity);
                foodQuantity = 0;
                foodText.text = foodQuantity.ToString();
                SetTargetPosition(FindUrbanCenter());
            }

            if (collision.CompareTag("UrbanCenter"))
            {
                UrbanCenter urbanCenter = collision.GetComponent<UrbanCenter>();
                foodQuantity = foodPerTravel;
                foodText.text = foodQuantity.ToString();
                SetTargetPosition(FindNearestGoldMine());
            }
        }
    }
}