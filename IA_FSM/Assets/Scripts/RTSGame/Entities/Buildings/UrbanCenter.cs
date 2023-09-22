using UnityEngine;

namespace RTSGame.Entities.Buildings
{
    public class UrbanCenter : MonoBehaviour
    {
        [Header("Gold")]
        [SerializeField] private TextMesh goldText;

        public int goldQuantity;

        private void Awake()
        {
            goldText.text = goldQuantity.ToString();
        }

        public void DeliverGold(int goldQuantity)
        {
            this.goldQuantity += goldQuantity;
            goldText.text = this.goldQuantity.ToString();
        }
    }
}