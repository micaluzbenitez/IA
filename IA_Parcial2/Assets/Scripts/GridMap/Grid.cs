using System;
using UnityEngine;

namespace GridMap
{
    public class Grid<TGridObject>
    {
        private int width;
        private int height;
        private float cellSize;
        private Vector3 originPosition;

        private TGridObject[,] gridArray;

        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            gridArray = new TGridObject[width, height];

            // Init grid array
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }

            // Debug
            bool showDebug = true;
            if (showDebug)
            {
                for (int x = 0; x < gridArray.GetLength(0); x++)
                {
                    for (int z = 0; z < gridArray.GetLength(1); z++)
                    {
                        // Gizmos Lines
                        Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x, z + 1), Color.black, 100f);
                        Debug.DrawLine(GetWorldPosition(x, z), GetWorldPosition(x + 1, z), Color.black, 100f);
                    }
                }

                // Gizmos Lines
                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.black, 100f);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.black, 100f);
            }
        }

        public Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(x, 0, z) * cellSize + originPosition;
        }
    }
}