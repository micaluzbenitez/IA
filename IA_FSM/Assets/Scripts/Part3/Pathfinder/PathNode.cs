using UnityEngine;
using Part3.GridMap;

namespace Part3.Pathfinder
{
    public class PathNode
    {
        private Grid<PathNode> grid;

        public int x;
        public int y;

        public int gCost;  // Walking cost from the start node
        public int hCost;  // Heuristic cost to reach end node
        public int fCost;  // gCost + hCost

        public bool isWalkable;
        public PathNode cameFromNode;

        public PathNode(Grid<PathNode> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            isWalkable = true;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
            grid.TriggerGridObjectChanged(x, y);
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}