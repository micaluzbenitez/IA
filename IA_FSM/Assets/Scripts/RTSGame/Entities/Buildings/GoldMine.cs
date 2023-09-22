using Toolbox;
using UnityEngine;
using RTSGame.Interfaces;

namespace RTSGame.Entities.Buildings
{
    public class GoldMine : MonoBehaviour, IInteractable
    {
        [Header("Gold")]
        [SerializeField] private int goldQuantity;

        private DebugText debugText;

        private void Start()
        {
            debugText = GetComponent<DebugText>();
            if (debugText)
            {
                debugText.SetParent(transform);
                debugText.Text.text = goldQuantity.ToString();
            }
        }

        public bool Interact(int goldQuantity)
        {
            if (this.goldQuantity <= 0) return false;

            this.goldQuantity--;
            if (debugText) debugText.Text.text = this.goldQuantity.ToString();
            return true;
        }
    }
}