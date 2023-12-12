using UnityEngine;

namespace Flocking
{
    public class Boid : MonoBehaviour
    {
        public float speed = 2.5f;
        public float turnSpeed = 5f;
        public Vector2 currentPosition;
        public CircleCollider2D circleCollider2D;

        [Header("Distances")]
        public float alignmentDistance = 0;
        public float cohesionDistance = 0;
        public float separationDistance = 0;
        public float obstacleDistance = 0;
        // Definen las distancias maximas a las que los boids interactuan con sus vecinos en los diferentes comportamientos

        [Header("Weights")]
        public float alignmentWeight = 0;
        public float cohesionWeight = 0;
        public float separationWeight = 0;
        public float obstacleWeight = 0;
        // Pesos para ajustar que tanto influye cada comportamiento en el flocking

        private FlockingManager fM;

        private void Start()
        {
            fM = FlockingManager.instance;
        }

        private void Update()
        {
            transform.position += transform.up * speed * Time.deltaTime;
            currentPosition = transform.position;
            transform.up = Vector3.Lerp(transform.up, ACS(), turnSpeed * Time.deltaTime);
        }

        public Vector2 ACS()
        {
            Vector2 ACS = fM.Alignment(this) * alignmentWeight + fM.Cohesion(this) * cohesionWeight +
                          fM.Separation(this) * separationWeight + fM.Direction(this, fM.flockPoint) +
                          fM.Obstacle(this) * obstacleWeight;

            ACS.Normalize();

            return ACS;
        }
    }
}