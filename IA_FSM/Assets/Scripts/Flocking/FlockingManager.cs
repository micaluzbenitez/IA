using System.Collections.Generic;
using UnityEngine;
using RTSGame.Entities.Agents;

namespace Flocking
{
    public class FlockingManager : MonoBehaviour
    {
        private Agent[] agents;

        public static FlockingManager instance;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            agents = FindObjectsOfType<Agent>();
        }

        // Hacia donde miran
        public Vector2 Alignment(Agent boid)
        {
            // Cada uno de los boids toma a todas las entidades que tiene en un radio
            List<Agent> insideRadiusBoids = GetInsideRadiusBoids(boid);
            Vector2 avg = Vector2.zero;

            // Toma hacia donde mira cada uno de ellos, calcula un promedio y lo normaliza
            foreach (Agent a in insideRadiusBoids)
            {
                avg += (Vector2)a.transform.up.normalized;
            }

            avg /= insideRadiusBoids.Count;
            avg.Normalize();
            return avg;
        }

        // Cuan alineado esta a sus compañeros, cuan en conjunto va con el resto
        public Vector2 Cohesion(Agent agent)
        {
            // Cada uno de los boids toma a todas las entidades que tiene en un radio
            List<Agent> insideRadiusBoids = GetInsideRadiusBoids(agent);
            Vector2 avg = Vector2.zero;

            // Calcula un promedio de su posicion
            foreach (Agent a in insideRadiusBoids)
            {
                avg += (Vector2)a.Position;
            }

            avg /= insideRadiusBoids.Count;

            // Y la normaliza sacando sacando la posicion relativa a la suya
            return (avg - (Vector2)agent.Position).normalized;
        }

        // Cuanto no se pisan con el resto
        public Vector2 Separation(Agent agent)
        {
            // Cada uno de los boids toma a todas las entidades que tiene en un radio
            List<Agent> insideRadiusBoids = GetInsideRadiusBoids(agent);
            Vector2 avg = Vector2.zero;

            // Los suma
            foreach (Agent a in insideRadiusBoids)
            {
                avg += (Vector2)(a.Position - agent.Position);

            }

            // Los separa, hace un promedio y lo normaliza
            avg /= insideRadiusBoids.Count;
            avg *= -1;
            avg.Normalize();
            return avg;
        }

        // Hacia donde van
        public Vector2 Direction(Agent agent, Vector3 target)
        {
            // parametro target = hacia donde mira
            // Suma hacia donde va
            return ((Vector2)target - (Vector2)agent.Position).normalized;
        }

        public List<Agent> GetInsideRadiusBoids(Agent boid)
        {
            List<Agent> insideRadiusBoids = new List<Agent>();

            foreach (Agent a in agents)
            {
                if (boid.CircleCollider2D.OverlapPoint(a.Position))
                {
                    insideRadiusBoids.Add(a);
                }
            }

            return insideRadiusBoids;
        }
    }
}