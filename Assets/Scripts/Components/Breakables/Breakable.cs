using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] private GameObject normalModel;
    [SerializeField] private Transform brokenModelPrefab;

    private AudioSource audioSource;
    private BoxCollider windowCollider;
    private bool hasBroken = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        windowCollider = GetComponent<BoxCollider>();
    }

    public void Break(Vector3 explosionOrigin, float force)
    {
        if (hasBroken)
        {
            return;
        }

        hasBroken = true;

        Audio.RandomizePitchAndVolume(audioSource);

        audioSource.Play();

        Destroy(normalModel);

        windowCollider.enabled = false;

        Transform brokenModelTransform = Instantiate(brokenModelPrefab, transform.position, transform.rotation);

        foreach (Transform child in brokenModelTransform)
        {
            if (child.TryGetComponent(out Rigidbody rigidbody))
            {
                Vector3 directionFromExplosion = (transform.position - explosionOrigin).normalized;

                rigidbody.velocity = directionFromExplosion * force;
            }
        }
    }
}
