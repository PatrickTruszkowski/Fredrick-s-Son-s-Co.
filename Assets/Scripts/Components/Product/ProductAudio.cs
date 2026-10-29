using UnityEngine;

public class ProductAudio : MonoBehaviour
{
    [SerializeField] private Product product = null;
    [SerializeField] private AudioClip batteryInsertSound = null;

    private const float DROP_SOUND_VOLUME = 0.5f;
    private const float BOUNCE_SOUND_VOLUME = 0.5f;
    private const float TAKE_SOUND_VOLUME = 0.5f;
    private const float USE_SOUND_VOLUME = 0.5f;

    private AudioSource audioSource = null;
    private ProductAsset productAsset;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        product.OnProductTaken += Product_OnProductTaken;
        product.OnProductHitGround += Product_OnProductHitGround;
        product.OnProductBounced += Product_OnProductBounced;

        if (product.TryGetComponent(out IUseable useableProduct))
        {
            useableProduct.OnUsed += UseableProduct_OnUsed;
        }

        if (product.TryGetComponent(out BatteryLife batteryLife))
        {
            batteryLife.OnBatteryLifeCharged += BatteryLife_OnBatteryLifeCharged;
        }

        productAsset = product.GetProductAsset();
    }

    private void Product_OnProductBounced(object sender, System.EventArgs e)
    {
        if (productAsset.bounceSound != null)
        {
            audioSource.pitch = productAsset.maximumDropPitch;
            audioSource.PlayOneShot(productAsset.bounceSound, BOUNCE_SOUND_VOLUME);
        }
    }

    private void Product_OnProductTaken(object sender, System.EventArgs e)
    {
        if (productAsset.takeSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(productAsset.takeSound, TAKE_SOUND_VOLUME);
        }
    }

    private void Product_OnProductHitGround(object sender, System.EventArgs e)
    {
        if (productAsset.dropSound != null)
        {
            audioSource.pitch = Random.Range(productAsset.minimumDropPitch, productAsset.maximumDropPitch);
            audioSource.PlayOneShot(productAsset.dropSound, DROP_SOUND_VOLUME);
        }
    }

    private void UseableProduct_OnUsed(object sender, System.EventArgs e)
    {
        if (productAsset.useSound != null)
        {
            audioSource.pitch = Random.Range(productAsset.minimumUsePitch, productAsset.maximumUsePitch);
            audioSource.PlayOneShot(productAsset.useSound, USE_SOUND_VOLUME);
        }
    }

    private void BatteryLife_OnBatteryLifeCharged(object sender, System.EventArgs e)
    {
        audioSource.PlayOneShot(batteryInsertSound, TAKE_SOUND_VOLUME);
    }
}
