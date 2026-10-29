using UnityEngine;

public class LoosePiece : MonoBehaviour
{
    [SerializeField] private Collider pieceCollider;
    
    private Rigidbody pieceRigidbody;

    private const float NO_MOVEMENT_DURATION = 2f;

    private float noMovementTimer = 0f;

    private void Awake()
    {
        if (pieceCollider == null)
        {
            if (TryGetComponent(out Collider collider))
            {
                pieceCollider = collider;
            }
            else
            {
                Debug.LogError("Loose piece has no collider.");
            }
        }

        pieceRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        ObjectPoolManager.Singleton.AddPiece(gameObject);
    }

    private void FixedUpdate()
    {
        if (pieceRigidbody.velocity.magnitude == 0f)
        {
            if (noMovementTimer < NO_MOVEMENT_DURATION)
            {
                noMovementTimer += Time.deltaTime;
                return;
            }

            Destroy(pieceCollider);
            Destroy(pieceRigidbody);
            Destroy(this);
        }
        else
        {
            noMovementTimer = 0f;
        }
    }
}
