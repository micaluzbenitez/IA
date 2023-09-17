using UnityEngine;
using Toolbox;

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
                gridInt.SetGridObject(MousePosition.GetMouseWorldPosition(), 56);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log(gridInt.GetGridObject(MousePosition.GetMouseWorldPosition()));
            }
        }
    }
}