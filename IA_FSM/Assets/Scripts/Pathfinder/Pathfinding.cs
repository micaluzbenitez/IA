using System.Collections.Generic;
using UnityEngine;
using Pathfinder.GridMap;
using RTSGame.Entities.Agents;

namespace Pathfinder
{
    public class Pathfinding
    {
        private const int MOVE_STRAIGHT_COST = 10;

        private Grid<PathNode> grid;
        private List<PathNode> openList;    // Nodos para buscar
        private List<PathNode> closedList;  // Nodos que ya fueron buscados

        public static Pathfinding Instance { get; private set; }
        public Pathfinding(int width, int height, float cellSize = 10f, Vector3 originPosition = default)
        {
            Instance = this;
            grid = new Grid<PathNode>(width, height, cellSize, originPosition, (Grid<PathNode> g, int x, int y) => new PathNode(g, x, y));

            // Seteo los tipos de nodos de manera random
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

        // Retorna el vector3 path para el jugador
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
            // Busco el nodo inicial y final
            PathNode startNode = grid.GetGridObject(startX, startY);
            PathNode endNode = grid.GetGridObject(endX, endY);

            openList = new List<PathNode> { startNode }; // Empiezo con el nodo inicial
            closedList = new List<PathNode>();

            // Inicializo la grid recorriendo todos los nodos
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

            while (openList.Count > 0) // Mientras todavia tengamos nodos por recorrer
            { 
                // Obtengo el nodo actual
                PathNode currentNode = GetLowestFCostNode(openList);

                if (currentNode == endNode)
                {
                    // Nodo final alcanzado
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // Recorremos los vecinos del nodo en cuestion
                foreach (PathNode neighbourNode in GetNeightbourList(currentNode))
                {
                    // Si el vecino esta en la lista de los que ya fueron buscados, saltamos al siguiente
                    if (closedList.Contains(neighbourNode)) continue;

                    // Compruebe si es un nodo transitable por el agente
                    bool notAgentWalkable = true;
                    for (int i = 0; i < agentPathNodeWalkables.Length; i++)
                    {
                        if (neighbourNode.pathNodeType == agentPathNodeWalkables[i].pathNodeType) notAgentWalkable = false;
                    }

                    // Compruebe si es un nodo transitable (si no es un obstaculo)
                    if (!neighbourNode.isWalkable || notAgentWalkable)
                    {
                        closedList.Add(neighbourNode);
                        continue;
                    }

                    // "tentativeGCost" es el G valor del nodo actual + la distancia del nodo actual al vecino en cuestion
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);

                    // Chequeamos si es menor al actual G valor almacenado en el nodo vecino
                    // Esto es para ver si hay un camino mas rapido desde el nodo actual al nodo vecino del que tuvimos en el chequeo anterior
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

            // Fuera de nodos en openList
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

        // Calculo la distancia hasta el nodo final * un costo
        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int yDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return MOVE_STRAIGHT_COST * remaining;
        }

        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            // El nodo actual es el que tiene menor F valor
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

            if (pathNode.available && pathNode.isWalkable)
            {
                pathNode.available = false;
                return true;
            }

            return false;
        }
    }
}