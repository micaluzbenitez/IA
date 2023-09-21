using System.Collections.Generic;
using UnityEngine;
using Pathfinder.GridMap;
using RTSGame;

namespace Pathfinder
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;

        private Grid<PathNode> grid;
        private List<PathNode> openList;    // Nodes queued up for searching
        private List<PathNode> closedList;  // Nodes that have already been searched

        public static Pathfinding Instance { get; private set; }
        public Pathfinding(int width, int height, float cellSize = 10f, Vector3 originPosition = default)
        {
            Instance = this;
            grid = new Grid<PathNode>(width, height, cellSize, originPosition, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GetNode(x, y).SetRandomType();
                }
            }
        }

        public Grid<PathNode> GetGrid()
        {
            return grid;
        }

        public PathNode GetNode(int x, int y)
        {
            return grid.GetGridObject(x, y);
        }

        // Return vector3 path for the player
        public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition, AgentPathNodes.PathNode_Walkable[] agentPathNodeWalkables)
        {
            grid.GetXY(startWorldPosition, out int startX, out int startY);
            grid.GetXY(endWorldPosition, out int endX, out int endY);

            List<PathNode> path = FindPath(startX, startY, endX, endY, agentPathNodeWalkables);

            if (path == null)
            {
                return null;
            }
            else
            {
                List<Vector3> vectorPath = new List<Vector3>();
                foreach (PathNode pathNode in path)
                {
                    vectorPath.Add(new Vector3(pathNode.x, pathNode.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
                }
                return vectorPath;
            }
        }

        public List<PathNode> FindPath(int startX, int startY, int endX, int endY, AgentPathNodes.PathNode_Walkable[] agentPathNodeWalkables)
        {
            PathNode startNode = grid.GetGridObject(startX, startY);
            PathNode endNode = grid.GetGridObject(endX, endY);

            openList = new List<PathNode> { startNode };
            closedList = new List<PathNode>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = grid.GetGridObject(x, y);
                    pathNode.gCost = int.MaxValue;
                    pathNode.CalculateFCost(GetAgentPathNodeCost(agentPathNodeWalkables, startNode));
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);            
            startNode.CalculateFCost(GetAgentPathNodeCost(agentPathNodeWalkables, startNode));

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    // Reached final node
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNode neighbourNode in GetNeightbourList(currentNode))
                {
                    if (closedList.Contains(neighbourNode)) continue;

                    // Check if is an agent walkable node
                    bool notAgentWalkable = true;
                    for (int i = 0; i < agentPathNodeWalkables.Length; i++)
                    {
                        if (neighbourNode.pathNodeType == agentPathNodeWalkables[i].pathNodeType) notAgentWalkable = false;
                    }

                    // Check if is a walkable node
                    if (!neighbourNode.isWalkable || notAgentWalkable)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        neighbourNode.CalculateFCost(GetAgentPathNodeCost(agentPathNodeWalkables, startNode));

                        if (!openList.Contains(neighbourNode)) openList.Add(neighbourNode);
                    }
                }
            }

            // Out of nodes on the openList
            return null;
        }

        private int GetAgentPathNodeCost(AgentPathNodes.PathNode_Walkable[] agentPathNodeWalkables, PathNode startNode)
        {
            for (int i = 0; i < agentPathNodeWalkables.Length; i++)
            {
                if (startNode.pathNodeType == agentPathNodeWalkables[i].pathNodeType)
                {
                    return agentPathNodeWalkables[i].cost;
                }
            }

            return 0;
        }

        private List<PathNode> GetNeightbourList(PathNode currentNode)
        {
            List<PathNode> neightbourList = new List<PathNode>();

            if (currentNode.y - 1 >= 0) neightbourList.Add(GetNode(currentNode.x, currentNode.y - 1));                        // Down
            if (currentNode.y + 1 < grid.GetHeight()) neightbourList.Add(GetNode(currentNode.x, currentNode.y + 1));          // Top
            if (currentNode.x - 1 >= 0) neightbourList.Add(GetNode(currentNode.x - 1, currentNode.y));                        // Left
            if (currentNode.x + 1 < grid.GetWidth()) neightbourList.Add(GetNode(currentNode.x + 1, currentNode.y));           // Right
                                                                                                                              
            return neightbourList;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;

            while (currentNode.cameFromNode != null)
            { 
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }

            path.Reverse();
            return path;
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_STRAIGHT_COST * remaining;
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];

            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }

            return lowestFCostNode;
        }

        public bool CheckAvailableNode(int x, int y)
        {
            PathNode pathNode = grid.GetGridObject(x, y);

            if (pathNode.available)
            {
                pathNode.available = false;
                return true;
            }

            return false;
        }
    }
}