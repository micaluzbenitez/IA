using System.Collections.Generic;
using UnityEngine;

namespace Flocking
{
    public class FlockingManager : MonoBehaviour
    {
        public enum CheckType // Comportamientos
        { 
            Alignment, 
            Cohesion, 
            Separation, 
            Obstacle
        }

        public static FlockingManager instance;

        [Header("Target")]
        public GameObject flockPoint;

        [Header("Obstacles")]
        public Transform[] flockObstacles;

        private Boid[] boids;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            boids = FindObjectsOfType<Boid>();
        }

        // Devuelve la direccion hacia donde miran los boids cercanos al boid dado
        public Vector2 Alignment(Boid boid)
        {
            // El boid dado toma a todas las entidades que tiene en su radio
            List<Boid> insideRadiusBoids = GetInsideRadiusBoids(boid, CheckType.Alignment);
            Vector2 avg = Vector2.zero;

            // Toma hacia donde mira cada uno de ellos
            foreach (Boid b in insideRadiusBoids)
            {
                avg += (Vector2)b.transform.up.normalized;
            }

            // Calcula un promedio y lo normaliza
            avg /= insideRadiusBoids.Count;
            avg.Normalize();

            // Devuelve la direccion promedio
            return avg; 
        }

        // Cuan alineado esta a sus compañeros, cuan en conjunto va con el resto
        public Vector2 Cohesion(Boid boid)
        {
            // El boid dado toma a todas las entidades que tiene en su radio
            List<Boid> insideRadiusBoids = GetInsideRadiusBoids(boid, CheckType.Cohesion);
            Vector2 avg = Vector2.zero;

            // Calcula un promedio de su posicion
            foreach (Boid b in insideRadiusBoids)
            {
                avg += b.currentPosition;
            }
            avg /= insideRadiusBoids.Count;

            // Y la normaliza sacando sacando la posicion relativa a la suya
            // Devuelve la direccion hacia la posicion promedio de los boids cercanos
            return (avg - boid.currentPosition).normalized; 
        }

        // Cuanto no se pisan con el resto, evita colisiones
        public Vector2 Separation(Boid boid)
        {
            // El boid dado toma a todas las entidades que tiene en su radio
            List<Boid> insideRadiusBoids = GetInsideRadiusBoids(boid, CheckType.Separation);
            Vector2 avg = Vector2.zero;

            // Los suma
            foreach (Boid b in insideRadiusBoids)
            {
                avg += b.currentPosition - boid.currentPosition;
            }

            // Los separa, hace un promedio y lo normaliza
            avg /= insideRadiusBoids.Count;
            avg *= -1;
            avg.Normalize();

            // Devuelve la direccion opuesta al promedio de las posiciones de los boids cercanos
            return avg; 
        }

        // Hacia donde van
        public Vector2 Direction(Boid boid, GameObject target)
        {
            // parametro target = hacia donde mira
            // Suma hacia donde va
            // Devuelve la direecion hacia el target
            return ((Vector2)target.transform.position - boid.currentPosition).normalized;
        }

        // Devuelve la lista de boids que estan dentro del radio dado, segun el comportamiento
        public List<Boid> GetInsideRadiusBoids(Boid boid, CheckType checkType)
        {
            List<Boid> insideRadiusBoids = new List<Boid>();

            foreach (Boid b in boids)
            {
                if (boid.circleCollider2D.OverlapPoint(b.currentPosition))
                {
                    float distance = Vector2.Distance(b.currentPosition, boid.currentPosition);
                    float maxDistance = 0;

                    switch (checkType)
                    {
                        case CheckType.Alignment:
                            maxDistance = boid.alignmentDistance;
                            break;
                        case CheckType.Cohesion:
                            maxDistance = boid.cohesionDistance;
                            break;
                        case CheckType.Separation:
                            maxDistance = boid.separationDistance;
                            break;
                    }

                    if (distance <= maxDistance) insideRadiusBoids.Add(b);
                }
            }

            return insideRadiusBoids;
        }

        // Cuanto no se pisa con los obstaculos, evita colisiones
        public Vector2 Obstacle(Boid boid)
        {
            // El boid dado toma a todos los obstaculos que tiene en su radio
            List<Vector2> insideRadiusBoids = GetInsideRadiusObstacles(boid);
            Vector2 avg = Vector2.zero;

            // Los suma
            foreach (Vector2 b in insideRadiusBoids)
            {
                avg += b - boid.currentPosition;
            }

            // Los separa, hace un promedio y lo normaliza
            avg /= insideRadiusBoids.Count;
            avg *= -1;
            avg.Normalize();

            // Devulve la direccion opuesta al promedio de las posiciones de los obstaculos cercanos
            return avg;
        }

        // Devuelve la lista de obstaculos que estan dentro del radio dado
        public List<Vector2> GetInsideRadiusObstacles(Boid boid)
        {
            List<Vector2> insideRadiusBoids = new List<Vector2>();

            foreach (Transform b in flockObstacles)
            {
                if (boid.circleCollider2D.OverlapPoint(b.position))
                {
                    float distance = Vector2.Distance(b.position, boid.currentPosition);
                    if (distance <= boid.obstacleDistance) insideRadiusBoids.Add(b.position);
                }
            }

            return insideRadiusBoids;
        }
    }
}