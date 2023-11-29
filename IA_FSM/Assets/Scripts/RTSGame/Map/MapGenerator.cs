using System;
using UnityEngine;
using Toolbox;
using Pathfinder;
using RTSGame.Entities.Buildings;
using System.Collections.Generic;

namespace RTSGame.Map
{
    public class MapGenerator : MonoBehaviourSingleton<MapGenerator>
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
        [SerializeField] private Vector2 originPosition;

        [Header("Path nodes")]
        [SerializeField] private PathNode_Visible[] pathNodeVisibles;

        [Header("Obstacles")]
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private int obstaclesQuantity;

        [Header("Gold mines")]
        [SerializeField] private GoldMine goldMinePrefab;
        [SerializeField] private int goldMinesQuantity;

        [Header("Urban center")]
        [SerializeField] private UrbanCenter urbanCenterPrefab;

        private Pathfinding pathfinding;

        public static List<GoldMine> goldMines = new List<GoldMine>();
        public static List<GoldMine> goldMinesBeingUsed = new List<GoldMine>();

        public static Vector2 MapDimensions;
        public static float CellSize;
        public static Vector2 OriginPosition;

        public override void Awake()
        {
            base.Awake();

            MapDimensions = new Vector2Int(width, height);
            CellSize = cellSize;
            OriginPosition = originPosition;

            pathfinding = new Pathfinding(width, height, cellSize, originPosition);
            CreateObstacles();
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

        private void CreateObstacles()
        {
            if (obstaclesQuantity <= 0) return;

            for (int i = 0; i < obstaclesQuantity; i++)
            {
                CreateEntity(obstaclePrefab, false);
            }
        }

        private void CreateGoldMines()
        {
            if (goldMinesQuantity <= 0) return;

            for (int i = 0; i < goldMinesQuantity; i++)
            {
                GameObject GO = CreateEntity(goldMinePrefab.gameObject);
                goldMines.Add(GO.GetComponent<GoldMine>());
            }
        }

        private void CreateUrbanCenter()
        {
            CreateEntity(urbanCenterPrefab.gameObject);
        }

        private GameObject CreateEntity(GameObject buildingPrefab, bool walkable = true)
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
            if (!walkable) pathfinding.GetNode(coords.x, coords.y).SetIsWalkable(!pathfinding.GetNode(coords.x, coords.y).isWalkable);
            GO.transform.localScale = Vector3.one * cellSize;
            return GO;
        }

        public void RemoveEmptyMine(GoldMine goldMine)
        {
            goldMines.Remove(goldMine);
            goldMine.gameObject.SetActive(false);
        }
    }
}