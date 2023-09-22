using System;
using UnityEngine;
using Pathfinder;
using RTSGame.Entities.Buildings;

namespace RTSGame.Map
{
    public class MapGenerator : MonoBehaviour
    {
        [Serializable]
        public class PathNode_Visible
        {
            public PathNode.PathNode_Type pathNodeType;
            public GameObject prefab;
        }

        [Header("Map")]
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField, Tooltip("Distance between map nodes")] private float cellSize;

        [Header("Path nodes")]
        public PathNode_Visible[] pathNodeVisibles;

        [Header("Gold mines")]
        [SerializeField] private GoldMine goldMinePrefab;
        [SerializeField] private int goldMinesQuantity;

        [Header("Urban center")]
        [SerializeField] private UrbanCenter urbanCenterPrefab;

        private Pathfinding pathfinding;

        private void Start()
        {
            pathfinding = new Pathfinding(width, height, cellSize);
            CreateGoldMines();
            CreateUrbanCenter();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 position = pathfinding.GetGrid().GetWorldPosition(x, y) + (Vector3.one * (cellSize / 2));

                    for (int i = 0; i < pathNodeVisibles.Length; i++)
                    {
                        if (pathfinding.GetNode(x, y).pathNodeType == pathNodeVisibles[i].pathNodeType && pathNodeVisibles[i].prefab)
                        {
                            GameObject GO = Instantiate(pathNodeVisibles[i].prefab, position, Quaternion.identity, transform);
                            GO.transform.localScale = Vector3.one * cellSize;
                        }
                    }
                }
            }
        }

        private void CreateGoldMines()
        {
            if (goldMinesQuantity <= 0) return;

            for (int i = 0; i < goldMinesQuantity; i++)
            {
                CreateBuilding(goldMinePrefab.gameObject);
            }
        }

        private void CreateUrbanCenter()
        {
            CreateBuilding(urbanCenterPrefab.gameObject);
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
            GameObject GO = Instantiate(buildingPrefab, position, Quaternion.identity, transform);
            GO.transform.localScale = Vector3.one * cellSize;
        }
    }
}