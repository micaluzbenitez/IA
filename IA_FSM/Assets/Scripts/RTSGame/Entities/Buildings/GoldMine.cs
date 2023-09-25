using System;
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
        private int villagersOn;

        private bool withVillagers = false;
        public bool WithVillagers => withVillagers;

        public Action<GoldMine> OnGoldMineEmpty;

        private void Awake()
        {
            goldText.text = goldQuantity.ToString();
            foodText.text = foodQuantity.ToString();
        }

        public bool ConsumeGold()
        {
            if (goldQuantity <= 0) return false;

            goldQuantity--;
            goldText.text = goldQuantity.ToString();

            if (goldQuantity <= 0)
            {
                OnGoldMineEmpty?.Invoke(this);
                withVillagers = false;
            }

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

        public void AddVillager()
        {
            villagersOn++;
            withVillagers = true;
        }

        public void RemoveVillager()
        {
            villagersOn--;
            if (villagersOn <= 0) withVillagers = false;
        }
    }
}