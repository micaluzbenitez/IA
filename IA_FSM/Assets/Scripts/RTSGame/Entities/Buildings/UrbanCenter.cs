using RTSGame.Interfaces;
using UnityEngine;

namespace RTSGame.Entities.Buildings
{
    public class UrbanCenter : MonoBehaviour, IInteractable
    {
        private int goldQuantity;
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
            this.goldQuantity += goldQuantity;
            if (debugText) debugText.Text.text = this.goldQuantity.ToString();
            return true;
        }
    }
}