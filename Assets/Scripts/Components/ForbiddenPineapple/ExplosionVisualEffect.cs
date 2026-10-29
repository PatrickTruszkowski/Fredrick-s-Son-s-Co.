using UnityEngine;

public class ExplosionVisualEffect : MonoBehaviour
{
    [SerializeField] private ForbiddenPineapple forbiddenPineapple;
    [SerializeField] private Transform explosionPrefab;

    private AudioSource audioSource;
    private bool explosionStart = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        forbiddenPineapple.OnExploded += ForbiddenPineapple_OnExploded;
    }

    private void Update()
    {
        if (explosionStart && !audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    private void ForbiddenPineapple_OnExploded(object sender, System.EventArgs e)
    {
        transform.parent = null;

        audioSource.Play();

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        PlayerCameraShaker.LocalSingleton.Shake(transform.position, ShakeIntensity.Extreme);

        explosionStart = true;
    }
}
