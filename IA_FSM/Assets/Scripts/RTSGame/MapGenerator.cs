using UnityEngine;
using Pathfinder;
using UnityEngine.UIElements;

namespace RTSGame
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Map")]
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField, Tooltip("Distance between map nodes")] private float cellSize;

        [Header("Gold mines")]
        [SerializeField] private GameObject goldMinePrefab;
        [SerializeField] private int goldMinesQuantity;

        [Header("Urban center")]
        [SerializeField] private GameObject urbanCenterPrefab;

        private Pathfinding pathfinding;

        private void Start()
        {
            pathfinding = new Pathfinding(width, height, cellSize);
            CreateGoldMines();
            CreateUrbanCenter();
        }

        private void CreateGoldMines()
        {
            if (goldMinesQuantity <= 0) return;

            for (int i = 0; i < goldMinesQuantity; i++)
            {
                CreateBuilding(goldMinePrefab);
            }
        }

        private void CreateUrbanCenter()
        {
            CreateBuilding(urbanCenterPrefab);
        }

        private void CreateBuilding(GameObject buildingPrefab)
        {
            Vector2Int coords;

            do
            {
                coords = pathfinding.GetGrid().GetRandomGridObject();
            }
            while (!pathfinding.CheckAvailableNode(coords.x, coords.y)); // Find an available node

            // Create building
            Vector2 position = pathfinding.GetGrid().GetWorldPosition(coords.x, coords.y) + (Vector3.one * (cellSize / 2));
            Instantiate(buildingPrefab, position, Quaternion.identity);
        }
    }
}