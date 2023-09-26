using RTSGame.Map;
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

        private Vector3 position;
        private int foodQuantity;
        private int villagersOn;
        private bool beingUsed = false;

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public bool BeingUsed => beingUsed;

        // Actions
        public Action OnGoldMineBeingUsed;
        public Action<GoldMine> OnGoldMineEmpty;

        private void Start()
        {
            position = transform.position;
            goldText.text = goldQuantity.ToString();
            foodText.text = foodQuantity.ToString();
        }

        private void UpdateText(TextMesh text, int quantity)
        {
            text.text = quantity.ToString();
        }

        public bool ConsumeGold()
        {
            if (goldQuantity <= 0) return false;

            goldQuantity--;
            UpdateText(goldText, goldQuantity);            

            if (goldQuantity <= 0)
            {
                OnGoldMineEmpty?.Invoke(this);
                MapGenerator.goldMinesBeingUsed.Remove(this);
                OnGoldMineBeingUsed?.Invoke();
                beingUsed = false;
            }

            return true;
        }

        public void DeliverFood(int foodQuantity)
        {
            this.foodQuantity += foodQuantity;
            UpdateText(foodText, this.foodQuantity);
        }
        
        public bool ConsumeFood()
        {
            if (foodQuantity <= 0) return false;

            foodQuantity--;
            UpdateText(foodText, foodQuantity);
            return true;
        }

        public void AddVillager()
        {
            villagersOn++;

            if (!beingUsed)
            {
                MapGenerator.goldMinesBeingUsed.Add(this);
                OnGoldMineBeingUsed?.Invoke();
                beingUsed = true;
            }
        }

        public void RemoveVillager()
        {
            villagersOn--;

            if (villagersOn <= 0 && beingUsed)
            {
                MapGenerator.goldMinesBeingUsed.Remove(this);
                OnGoldMineBeingUsed?.Invoke();
                beingUsed = false;
            }
        }
    }
}