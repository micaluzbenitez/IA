using RTSGame.Entities.Agents;
using RTSGame.Map;
using System;
using System.Collections.Generic;
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

        private SpriteRenderer spriteRenderer;
        private Vector3 position;
        private int foodQuantity;
        private int villagersOn;

        private bool beingUsed = false;
        private bool withGold;
        private List<Villager> villagers = new List<Villager>();

        private string goldQuantityText;
        private string foodQuantityText;

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public bool BeingUsed => beingUsed;
        public bool WithGold => withGold;
        public List<Villager> Villagers => villagers;

        // Actions
        public Action OnGoldMineBeingUsed;
        public Action OnGoldMineEmpty;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            position = transform.position;
            goldQuantityText = goldQuantity.ToString();
            foodQuantityText = foodQuantity.ToString();
        }

        private void Update()
        {
            goldText.text = goldQuantityText;
            foodText.text = foodQuantityText;

            if (goldQuantity > 0) withGold = true;
            else withGold = false;
        }

        public bool ConsumeGold()
        {
            if (goldQuantity <= 0) return false;

            goldQuantity--;
            goldQuantityText = goldQuantity.ToString();            

            if (goldQuantity <= 0)
            {
                MapGenerator.Instance.RemoveEmptyMine(this);
                MapGenerator.goldMinesBeingUsed.Remove(this);
                OnGoldMineEmpty?.Invoke();
                OnGoldMineBeingUsed?.Invoke();
                beingUsed = false;
            }

            return true;
        }

        public void DeliverFood(int foodQuantity)
        {
            this.foodQuantity += foodQuantity;
            foodQuantityText = this.foodQuantity.ToString();
        }
        
        public bool ConsumeFood()
        {
            if (foodQuantity <= 0) return false;

            foodQuantity--;
            foodQuantityText = foodQuantity.ToString();
            return true;
        }

        public void AddVillager(Villager villager)
        {
            villagersOn++;
            villagers.Add(villager);

            if (!beingUsed)
            {
                MapGenerator.goldMinesBeingUsed.Add(this);
                OnGoldMineBeingUsed?.Invoke();
                beingUsed = true;
            }
        }

        public void RemoveVillager(Villager villager)
        {
            villagersOn--;
            villagers.Remove(villager);

            if (villagersOn <= 0 && beingUsed)
            {
                MapGenerator.goldMinesBeingUsed.Remove(this);
                OnGoldMineBeingUsed?.Invoke();
                beingUsed = false;
            }
        }
    }
}