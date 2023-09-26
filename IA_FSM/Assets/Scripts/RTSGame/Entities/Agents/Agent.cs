using FiniteStateMachine;
using RTSGame.Entities.Buildings;
using UnityEngine;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents
{
    public class Agent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] protected float speed = 5f;

        [Header("Voronoi")]
        [SerializeField] protected Voronoi voronoi = null;
        [SerializeField] protected bool drawVoronoi;

        protected AgentPathNodes agentPathNodes;
        protected FSM fsm;

        protected UrbanCenter urbanCenter;
        protected Vector3 position;
        protected float deltaTime;

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        protected virtual void Awake()
        {
            urbanCenter = FindObjectOfType<UrbanCenter>();
            agentPathNodes = GetComponent<AgentPathNodes>();
        }

        protected virtual void Start()
        {
            position = transform.position;
        }

        protected virtual void Update()
        {
            transform.position = position;
            deltaTime = Time.deltaTime;
        }

        protected virtual void OnDrawGizmos()
        {
            if (drawVoronoi) voronoi.Draw();
        }

        public virtual void UpdateAgent() 
        {
            fsm.Update();
        }
    }
}