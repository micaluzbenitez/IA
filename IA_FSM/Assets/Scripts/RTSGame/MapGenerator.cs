using Pathfinder;
using Pathfinder.GridMap;
using Toolbox;
using UnityEngine;

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
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPosition = MousePosition.GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

                Vector2 position = pathfinding.GetGrid().GetWorldPosition(x, y) + (Vector3.one * (cellSize / 2));
                Instantiate(cube, position, Quaternion.identity);
            }
        }
    }
}