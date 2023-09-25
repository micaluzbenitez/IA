using System;
using UnityEngine;
using Toolbox;
using Pathfinder;
using VoronoiDiagram;
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

        [Header("Gold mines")]
        [SerializeField] private GoldMine goldMinePrefab;
        [SerializeField] private int goldMinesQuantity;

        [Header("Urban center")]
        [SerializeField] private UrbanCenter urbanCenterPrefab;

        [Header("Voronoi diagram")]
        [SerializeField] private Voronoi voronoi = null;

        private Pathfinding pathfinding;
        public static List<GoldMine> goldMines = new List<GoldMine>();

        public static Vector2 MapDimensions;
        public static float CellSize;
        public static Vector2 OriginPosition;

        private void Start()
        {
            MapDimensions = new Vector2Int(width, height);
            CellSize = cellSize;
            OriginPosition = originPosition;

            pathfinding = new Pathfinding(width, height, cellSize, originPosition);
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

            voronoi.Init();
            voronoi.SetVoronoi(goldMines);
        }

        private void CreateGoldMines()
        {
            if (goldMinesQuantity <= 0) return;

            for (int i = 0; i < goldMinesQuantity; i++)
            {
                GameObject GO = CreateBuilding(goldMinePrefab.gameObject);
                GoldMine goldMine = GO.GetComponent<GoldMine>();
                goldMine.OnGoldMineEmpty += RemoveEmptyMine;
                goldMines.Add(goldMine);
            }
        }

        private void CreateUrbanCenter()
        {
            CreateBuilding(urbanCenterPrefab.gameObject);
        }

        private GameObject CreateBuilding(GameObject buildingPrefab)
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
            return GO;
        }

        private void RemoveEmptyMine(GoldMine goldMine)
        {
            goldMines.Remove(goldMine);
            goldMine.OnGoldMineEmpty -= RemoveEmptyMine;
            goldMine.gameObject.SetActive(false);
            voronoi.SetVoronoi(goldMines);
        }

        public GoldMine GetMineCloser(Vector3 minerPos)
        {
            return voronoi.GetMineCloser(minerPos);
        }
    }
}