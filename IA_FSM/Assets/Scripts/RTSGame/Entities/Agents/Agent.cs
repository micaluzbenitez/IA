using System.Collections.Generic;
using UnityEngine;
using Pathfinder;

namespace RTSGame.Entities.Agents
{
    [RequireComponent(typeof(AgentPathNodes))]
    public class Agent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] protected float speed = 5f;
        [SerializeField] protected float distancePerObjetive = 2.5f;

        protected AgentPathNodes agentPathNodes;
        protected int currentPathIndex;
        protected List<Vector3> pathVectorList;

        protected virtual void Awake()
        {
            agentPathNodes = GetComponent<AgentPathNodes>();
        }

        protected virtual void Update()
        {
            HandleMovement();
        }

        protected void HandleMovement()
        {
            if (pathVectorList != null)
            {
                Vector3 targetPosition = pathVectorList[currentPathIndex];

                if (Vector3.Distance(transform.position, targetPosition) > distancePerObjetive)
                {
                    Vector3 moveDir = (targetPosition - transform.position).normalized;
                    transform.position = transform.position + moveDir * speed * Time.deltaTime;
                }
                else
                {
                    currentPathIndex++;
                    if (currentPathIndex >= pathVectorList.Count) pathVectorList = null; // Stop moving
                }
            }
        }

        public void SetTargetPosition(Vector3 targetPosition)
        {
            currentPathIndex = 0;
            pathVectorList = Pathfinding.Instance.FindPath(transform.position, targetPosition, agentPathNodes.pathNodeWalkables);

            if (pathVectorList != null && pathVectorList.Count > 1)
            {
                pathVectorList.RemoveAt(0);
            }
        }
    }
}