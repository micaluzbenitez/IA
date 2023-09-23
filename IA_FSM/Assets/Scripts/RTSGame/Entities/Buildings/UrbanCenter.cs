using System;
using UnityEngine;

namespace RTSGame.Entities.Buildings
{
    public class UrbanCenter : MonoBehaviour
    {
        [Serializable]
        public class Agent
        {
            public string tag;
            public GameObject prefab;
            public int quantity;
        }

        [Header("Agents")]
        [SerializeField] private Agent[] agents;

        [Header("Gold")]
        [SerializeField] private TextMesh goldText;

        private int goldQuantity;

        private void Awake()
        {
            goldText.text = goldQuantity.ToString();
        }

        private void Start()
        {
            for (int i = 0; i < agents.Length; i++)
            {
                for (int j = 0; j < agents[i].quantity; j++)
                {
                    Instantiate(agents[i].prefab, transform.position, Quaternion.identity);
                }
            }
        }

        public void DeliverGold(int goldQuantity)
        {
            this.goldQuantity += goldQuantity;
            goldText.text = this.goldQuantity.ToString();
        }
    }
}