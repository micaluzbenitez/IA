using System.Collections.Generic;
using UnityEngine;
using Pathfinder;
using Toolbox;

namespace RTSGame
{
    [RequireComponent(typeof(AgentPathNodes))]
    public class Agent : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 5f;

        private AgentPathNodes agentPathNodes;
        private int currentPathIndex;
        private List<Vector3> pathVectorList;

        private void Awake()
        {
            agentPathNodes = GetComponent<AgentPathNodes>();
        }

        private void Update()
        {
            HandleMovement();

            if (Input.GetMouseButton(0))
            {
                Vector3 mouseWorldPosition = MousePosition.GetMouseWorldPosition();
                SetTargetPosition(mouseWorldPosition);
            }
        }

        private void HandleMovement()
        {
            if (pathVectorList != null)
            {
                Vector3 targetPosition = pathVectorList[currentPathIndex];

                if (Vector3.Distance(transform.position, targetPosition) > 1f)
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