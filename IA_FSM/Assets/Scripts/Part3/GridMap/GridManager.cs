using UnityEngine;

namespace Part3.GridMap
{
    public class GridManager : MonoBehaviour
    {
        private Grid grid;

        private void Start()
        {
            grid = new Grid(4, 2, 10f, new Vector3(-20, 0));
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                grid.SetValue(GetMouseWorldPosition(), 56);
            }

            if (Input.GetMouseButtonDown(1))
            {
                grid.GetValue(GetMouseWorldPosition());
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
            screenPosition.z = -worldCamera.transform.position.z;
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
        #endregion
    }
}