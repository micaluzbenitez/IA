using System.Collections.Generic;
using UnityEngine;

namespace Part3.Pathfinder
{
    public class PathfinderTesting : MonoBehaviour
    {
        private Pathfinding pathfinding;

        private void Start()
        {
            pathfinding = new Pathfinding(10, 10);
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mouseWorldPosition = GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

                List<PathNode> path = pathfinding.FindPath(0, 0, x, y);

                if (path != null)
                {
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 10f + Vector3.one * 10f, 
                                       new Vector3(path[i+1].x, path[i+1].y) * 10f + Vector3.one * 10f, 
                                       Color.green);
                    }
                }
            }

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