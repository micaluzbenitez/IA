using System.Collections.Generic;
using UnityEngine;
using Pathfinder.GridMap;
using UnityEngine.UIElements;

namespace Pathfinder
{
    public class PathNode
    {
        private Grid<PathNode> grid;

        public int x;
        public int y;

        public int gCost;  // Costo de caminar desde el nodo de inicio
        public int hCost;  // Costo sumado (heuristico) para llegar al nodo final
        public int fCost;  // gCost + hCost

        public bool available;
        public bool isWalkable;
        public PathNode cameFromNode;

        public int GCost
        {
            get { return gCost; }
            set { gCost = value; }
        }
        public int HCost
        {
            get { return hCost; }
            set { hCost = value; }
        }
        public int FCost
        {
            get { return fCost; }
            set { fCost = value; }
        }

        #region PathNode_Type
        public enum PathNode_Type
        {
            GRASS,
            MUD,
            WATER
        }

        private readonly Dictionary<PathNode_Type, int> pathNodeCosts = new Dictionary<PathNode_Type, int>
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
            available = true;
            isWalkable = true;
        }

        public void SetRandomType()
        {            
            float[] nodeTypePercentages = { 80f, 10f, 10f }; // Define los porcentajes para cada tipo de nodo.
            float randomValue = Random.Range(0f, 100f);

            // Encuentra el tipo de nodo según el valor aleatorio
            int nodeTypeIndex = 0;
            foreach (float percentage in nodeTypePercentages)
            {
                if (randomValue <= percentage)
                {
                    pathNodeType = (PathNode_Type)nodeTypeIndex;
                    break;
                }
                randomValue -= percentage;
                nodeTypeIndex++;
            }

            // Notifica el cambio
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

        public void CalculateFCost(int agentCost)
        {
            fCost = gCost + hCost + agentCost + GetPathNodeCost();
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}