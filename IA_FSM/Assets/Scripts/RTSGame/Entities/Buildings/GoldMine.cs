using UnityEngine;

namespace RTSGame.Entities.Buildings
{
    public class GoldMine : MonoBehaviour
    {
        [Header("Gold")]
        [SerializeField] private TextMesh goldText;
        [SerializeField] private int goldQuantity;

        [Header("Food")]
        [SerializeField] private TextMesh foodText;

        private int foodQuantity;

        private bool withVillagers = false;
        public bool WithVillagers => withVillagers;

        private void Awake()
        {
            goldText.text = goldQuantity.ToString();
            foodText.text = foodQuantity.ToString();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L)) DeliverFood(1);
        }

        public bool HasGold()
        {
            if (goldQuantity > 0) return true;
            else return false;
        }

        public bool ConsumeGold()
        {
            if (goldQuantity <= 0) return false;

            goldQuantity--;
            goldText.text = goldQuantity.ToString();
            return true;
        }

        public void DeliverFood(int foodQuantity)
        {
            this.foodQuantity += foodQuantity;
            foodText.text = this.foodQuantity.ToString();
        }
        
        public bool ConsumeFood()
        {
            if (foodQuantity <= 0) return false;

            foodQuantity--;
            foodText.text = foodQuantity.ToString();
            return true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Villager")) withVillagers = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Villager")) withVillagers = false;
        }
    }
}