using UnityEngine;

namespace Part1.AI
{
    public class Player : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        [SerializeField] private float speed;

        [Header("HP")]
        [SerializeField] private float hp;

        [Header("Damage")]
        [SerializeField] private int damage;

        private Vector3 movementVector = Vector3.zero;
        private Rigidbody playerRigidbody = null;

        private void Awake()
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Movement();
        }

        private void Movement()
        {
            movementVector.x = Input.GetAxis("Horizontal");
            movementVector.z = Input.GetAxis("Vertical");

            playerRigidbody.velocity = movementVector.normalized * speed;
        }

        public void TakeDamage(float damage)
        {
            if (hp <= 0) return;

            hp -= damage;
            if (hp < 0) hp = 0;
        }
    }
}