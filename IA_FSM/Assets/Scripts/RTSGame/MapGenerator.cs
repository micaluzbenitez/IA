using UnityEngine;
using Pathfinder;
using Toolbox;

namespace RTSGame
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map")]
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField, Tooltip("Distance between map nodes")] private float cellSize;

        [Header("Gold mines")]
        [SerializeField] private int goldMinesQuantity;

        public GameObject cube;

        private Pathfinding pathfinding;

        private void Start()
        {
            pathfinding = new Pathfinding(width, height, cellSize);
            CreateGoldMines();
        }

        private void CreateGoldMines()
        {
            if (goldMinesQuantity <= 0) return;

            for (int i = 0; i < goldMinesQuantity; i++)
            {
                pathfinding.GetGrid().GetRandomGridObject(out int x, out int y);
                Vector2 position = pathfinding.GetGrid().GetWorldPosition(x, y) + (Vector3.one * (cellSize / 2));
                Instantiate(cube, position, Quaternion.identity);
            }
        }
    }
}