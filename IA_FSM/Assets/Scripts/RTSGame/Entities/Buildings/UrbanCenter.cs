using System;
using System.Collections;
using UnityEngine;
using FiniteStateMachine.Multithreading;

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

        private Vector3 position;
        private int goldQuantity;
        private string goldQuantityText;

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        private void Start()
        {
            position = transform.position;
            goldQuantityText = goldQuantity.ToString();
            StartCoroutine(SpawnAgents());
        }

        private void Update()
        {
            goldText.text = goldQuantityText;
        }

        private IEnumerator SpawnAgents()
        {
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < agents.Length; i++)
            {
                for (int j = 0; j < agents[i].quantity; j++)
                {
                    GameObject GO = Instantiate(agents[i].prefab, transform.position + positionOffset, Quaternion.identity);
                    Agents.Agent agent = GO.GetComponent<Agents.Agent>();
                    if (agent) AgentsAdmin.Instance.AddAgent(agent);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }

        public void DeliverGold(int goldQuantity)
        {
            this.goldQuantity += goldQuantity;
            goldQuantityText = this.goldQuantity.ToString();
        }
    }
}