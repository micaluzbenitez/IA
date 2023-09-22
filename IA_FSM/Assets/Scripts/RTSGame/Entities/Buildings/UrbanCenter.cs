using RTSGame.Interfaces;
using UnityEngine;

namespace RTSGame.Entities.Buildings
{
    public class UrbanCenter : MonoBehaviour, IInteractable
    {
        [Header("Gold")]
        [SerializeField] private TextMesh text;

        public int goldQuantity;

        private void Awake()
        {
            text.text = goldQuantity.ToString();
        }

        public bool Interact(int goldQuantity)
        {
            this.goldQuantity += goldQuantity;
            text.text = this.goldQuantity.ToString();
            return true;
        }
    }
}