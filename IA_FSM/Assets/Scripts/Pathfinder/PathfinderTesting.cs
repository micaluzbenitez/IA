using System.Collections.Generic;
using UnityEngine;

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
                Vector3 mouseWorldPosition = GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
                player.SetTargetPosition(mouseWorldPosition);
            }

            // Add obstacle
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPosition = GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
                pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
            }
        }





        #region MOUSE_POSITION
        /// Get mouse position in world with Z = 0f
        private static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }

        private static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            screenPosition.z = Mathf.Abs(worldCamera.transform.position.z);
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
        #endregion
    }
}