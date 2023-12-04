using FiniteStateMachine;
using Flocking;
using RTSGame.Entities.Buildings;
using UnityEngine;
using VoronoiDiagram;

namespace RTSGame.Entities.Agents
{
    public class Agent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] protected float speed = 5f;
        [SerializeField] protected float turnSpeed = 5f;

        [Header("Voronoi")]
        [SerializeField] protected Voronoi voronoi = null;
        [SerializeField] protected bool drawVoronoi;

        [Header("Flocking")]
        [SerializeField] protected CircleCollider2D circleCollider2D;
        
        protected AgentPathNodes agentPathNodes;
        protected FSM fsm;
        protected Vector3 target;

        protected UrbanCenter urbanCenter;
        protected Vector3 position;
        protected float deltaTime;

        // Properties
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public float DeltaTime
        {
            get { return deltaTime; }
        }
        public UrbanCenter UrbanCenter
        {
            get { return urbanCenter; }
        }
        public CircleCollider2D CircleCollider2D
        {
            get { return circleCollider2D; }
        }
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        }

        protected virtual void Start()
        {
            urbanCenter = FindObjectOfType<UrbanCenter>();
            agentPathNodes = GetComponent<AgentPathNodes>();
            position = transform.position;
        }

        protected virtual void Update()
        {
            deltaTime = Time.deltaTime;
            transform.position = position;
            transform.up = Vector3.Lerp(transform.up, ACS(), turnSpeed * Time.deltaTime);
        }

        protected virtual void OnDrawGizmos()
        {
            if (drawVoronoi) voronoi.Draw();
        }

        public virtual void UpdateAgent() 
        {
            fsm.Update();
        }

        public Vector2 ACS()
        {
            Vector2 ACS = FlockingManager.instance.Alignment(this) + 
                          FlockingManager.instance.Cohesion(this) + 
                          FlockingManager.instance.Separation(this) + 
                          FlockingManager.instance.Direction(this, target);

            ACS.Normalize();

            return ACS;
        }
    }
}