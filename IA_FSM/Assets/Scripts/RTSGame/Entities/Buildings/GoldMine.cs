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

        public bool Interact()
        {
            if (goldQuantity <= 0) return false;

            goldQuantity--;
            if (debugText) debugText.Text.text = goldQuantity.ToString();
            return true;
        }
    }
}