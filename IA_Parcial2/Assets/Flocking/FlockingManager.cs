using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    public class FlockingManager : MonoBehaviour
    {
        public static FlockingManager instance;
        public GameObject flockPoint;

        private Boid[] boids;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            boids = FindObjectsOfType<Boid>();
        }

        // Hacia donde miran
        public Vector2 Alignment(Boid boid)
        {
            // Cada uno de los boids toma a todas las entidades que tiene en un radio
            List<Boid> insideRadiusBoids = GetInsideRadiusBoids(boid);
            Vector2 avg = Vector2.zero;

            // Toma hacia donde mira cada uno de ellos, calcula un promedio y lo normaliza
            foreach (Boid b in insideRadiusBoids)
            {
                avg += (Vector2)b.transform.up.normalized;
            }

            avg /= insideRadiusBoids.Count;
            avg.Normalize();
            return avg;
        }

        // Cuan alineado esta a sus compañeros, cuan en conjunto va con el resto
        public Vector2 Cohesion(Boid boid)
        {
            // Cada uno de los boids toma a todas las entidades que tiene en un radio
            List<Boid> insideRadiusBoids = GetInsideRadiusBoids(boid);
            Vector2 avg = Vector2.zero;

            // Calcula un promedio de su posicion
            foreach (Boid b in insideRadiusBoids)
            {
                avg += b.currentPosition;
            }

            avg /= insideRadiusBoids.Count;

            // Y la normaliza sacando sacando la posicion relativa a la suya
            return (avg - boid.currentPosition).normalized;
        }

        // Cuanto no se pisan con el resto
        public Vector2 Separation(Boid boid)
        {
            // Cada uno de los boids toma a todas las entidades que tiene en un radio
            List<Boid> insideRadiusBoids = GetInsideRadiusBoids(boid);
            Vector2 avg = Vector2.zero;

            // Los suma
            foreach (Boid b in insideRadiusBoids)
            {
                avg += (b.currentPosition - boid.currentPosition);

            }

            // Los separa, hace un promedio y lo normaliza
            avg /= insideRadiusBoids.Count;
            avg *= -1;
            avg.Normalize();
            return avg;
        }

        // Hacia donde van
        public Vector2 Direction(Boid boid, GameObject target)
        {

            // parametro target = hacia donde mira
            // Suma hacia donde va
            return ((Vector2)target.transform.position - boid.currentPosition).normalized;
        }

        public List<Boid> GetInsideRadiusBoids(Boid boid)
        {
            List<Boid> insideRadiusBoids = new List<Boid>();

            foreach (Boid b in boids)
            {
                if (boid.circleCollider2D.OverlapPoint(b.currentPosition))
                {
                    insideRadiusBoids.Add(b);
                }
            }

            return insideRadiusBoids;
        }
    }
}