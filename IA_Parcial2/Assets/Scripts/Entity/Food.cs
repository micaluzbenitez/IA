using UnityEngine;

public class Food : MonoBehaviour
{
    private Vector2Int index = Vector2Int.zero;

    public Vector2Int Index => index;

    public void Init(Vector2Int index)
    {
        this.index = index;
    }
}