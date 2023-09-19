using System.Collections.Generic;
using UnityEngine;
using Pathfinder.GridMap;

namespace Pathfinder
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

        #region PathNode_Type
        public enum PathNode_Type
        {
            GRASS,
            MUD,
            WATER
        }

        private static readonly Dictionary<PathNode_Type, int> pathNodeCosts = new Dictionary<PathNode_Type, int>
        {
            { PathNode_Type.GRASS, 0 },
            { PathNode_Type.MUD, 5 },
            { PathNode_Type.WATER, 10 }
        };

        public PathNode_Type pathNodeType = PathNode_Type.GRASS;
        #endregion

        public PathNode(Grid<PathNode> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            isWalkable = true;
        }

        public void SetRandomType()
        {
            pathNodeType = (PathNode_Type)Random.Range(0, System.Enum.GetValues(typeof(PathNode_Type)).Length);
            grid.TriggerGridObjectChanged(x, y, pathNodeType.ToString());
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
            grid.TriggerGridObjectChanged(x, y, "OBS");
        }

        public int GetPathNodeCost()
        {
            if (pathNodeCosts.TryGetValue(pathNodeType, out int cost))
            {
                return cost;
            }

            return 0;
        }

        public void CalculateFCost()
        {
            fCost = gCost + hCost + GetPathNodeCost();
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}