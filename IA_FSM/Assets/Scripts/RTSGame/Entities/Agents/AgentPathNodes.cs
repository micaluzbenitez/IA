using Pathfinder;
using System;
using UnityEngine;

namespace RTSGame.Entities.Agents
{
    public class AgentPathNodes : MonoBehaviour
    {
        [Serializable]
        public class PathNode_Walkable
        {
            public PathNode.PathNode_Type pathNodeType;
            public int cost;
        }

        [Header("Path nodes")]
        public PathNode_Walkable[] pathNodeWalkables;
    }
}