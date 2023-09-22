using Toolbox;
using UnityEngine;

namespace RTSGame
{
    public class DebugText : MonoBehaviour
    {
        [Header("Debug text")]
        [SerializeField] private int textSize = 25;
        [SerializeField] private Color textColor = Color.black;
        [SerializeField] private Vector3 textOffset = new Vector3(0, 3.7f, 0);

        private TextMesh text;
        public TextMesh Text => text;

        private void Awake()
        {
            Vector3 position = transform.position + textOffset;
            text = WorldText.CreateWorldText("", null, position, textSize, textColor, TextAnchor.MiddleCenter);
        }

        public void SetParent(Transform parent)
        {
            text.transform.SetParent(parent, true);
        }
    }
}