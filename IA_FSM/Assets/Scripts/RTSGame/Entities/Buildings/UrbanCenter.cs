using System;
using System.Collections;
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
        [SerializeField] Vector3 positionOffset;
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
            StartCoroutine(SpawnAgents());
        }

        private IEnumerator SpawnAgents()
        {
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < agents.Length; i++)
            {
                for (int j = 0; j < agents[i].quantity; j++)
                {
                    Instantiate(agents[i].prefab, transform.position + positionOffset, Quaternion.identity);
                    yield return new WaitForSeconds(0.2f);
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