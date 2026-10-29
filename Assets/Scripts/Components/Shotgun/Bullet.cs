using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float shotForce;

    private const float MAX_ACTIVE_DURATION = 15f;

    private Rigidbody bulletRigidbody;
    private AudioSource audioSource;
    private float activeTimer = 0f;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        bulletRigidbody.AddForce(transform.forward * shotForce, ForceMode.Impulse);
    }

    private void Update()
    {
        if (activeTimer < MAX_ACTIVE_DURATION)
        {
            activeTimer += Time.deltaTime;
            return;
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Breakable breakable))
        {
            breakable.Break(transform.position, 8f);
        }

        if (collision.transform.TryGetComponent(out HealthSystem healthSystem))
        {
            healthSystem.Damage(DamageStrength.Normal);
        }

        AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);

        Destroy(gameObject);
    }
}
