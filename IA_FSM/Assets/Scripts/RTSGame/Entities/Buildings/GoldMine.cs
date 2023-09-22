using Toolbox;
using UnityEngine;
using RTSGame.Interfaces;

namespace RTSGame.Entities.Buildings
{
    public class GoldMine : MonoBehaviour, IInteractable
    {
        [Header("Gold")]
        [SerializeField] private TextMesh text;
        [SerializeField] private int goldQuantity;

        private void Awake()
        {
            text.text = goldQuantity.ToString();
        }

        public bool Interact(int goldQuantity)
        {
            if (this.goldQuantity <= 0) return false;

            this.goldQuantity--;
            text.text = this.goldQuantity.ToString();
            return true;
        }
    }
}