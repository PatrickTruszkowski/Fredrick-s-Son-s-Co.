using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Singleton { get; private set; }

    private const int MAX_POOL_COUNT = 500;

    private Queue<GameObject> pieceQueue = new();

    private void Awake()
    {
        Singleton = this;
    }

    public void AddPiece(GameObject gameObject)
    {
        pieceQueue.Enqueue(gameObject);

        if (pieceQueue.Count > MAX_POOL_COUNT)
        {
            Destroy(pieceQueue.Dequeue());
        }
    }
}
