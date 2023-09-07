using UnityEngine;

namespace Pathfinder.GridMap
{
    public class GridTesting : MonoBehaviour
    {
        private Grid<int> gridInt;
        private Grid<bool> gridBool;

        private void Start()
        {
            gridInt = new Grid<int>(4, 2, 10f, new Vector3(-20, 0), (Grid<int> grid, int x, int y) => new int());
            gridBool = new Grid<bool>(4, 2, 10f, new Vector3(-20, -20), (Grid<bool> grid, int x, int y) => new bool());
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                gridInt.SetGridObject(GetMouseWorldPosition(), 56);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log(gridInt.GetGridObject(GetMouseWorldPosition()));
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