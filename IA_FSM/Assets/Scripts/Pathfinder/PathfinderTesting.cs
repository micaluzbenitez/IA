using UnityEngine;
using Toolbox;

namespace Pathfinder
{
    public class PathfinderTesting : MonoBehaviour
    {
        [SerializeField] private Player player;
        private Pathfinding pathfinding;

        private void Start()
        {
            pathfinding = new Pathfinding(10, 10);
        }

        private void Update()
        {
            // Move player
            if (Input.GetMouseButton(0))
            {
                Vector3 mouseWorldPosition = MousePosition.GetMouseWorldPosition();
                player.SetTargetPosition(mouseWorldPosition);
            }

            // Add obstacle
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPosition = MousePosition.GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
                pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
            }
        }
    }
}