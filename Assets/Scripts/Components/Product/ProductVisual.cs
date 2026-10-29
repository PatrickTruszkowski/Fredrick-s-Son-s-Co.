using UnityEngine;

public class ProductVisual : MonoBehaviour
{
    [SerializeField] private Product product = null;
    [SerializeField] private GameObject guideLightGameObject;

    private const string HIT_GROUND = "Hit Ground";

    private Animator animatior = null;

    private void Awake()
    {
        animatior = GetComponent<Animator>();
    }

    private void Start()
    {
        product.OnProductHitGround += Product_OnProductHitGround;
        product.OnProductTaken += Product_OnProductTaken;
        product.OnProductBounced += Product_OnProductBounced;
        product.OnProductPocketed += Product_OnProductPocketed;

        guideLightGameObject.SetActive(!product.HasInteraction());
    }

    private void Product_OnProductHitGround(object sender, System.EventArgs e)
    {
        animatior.SetTrigger(HIT_GROUND);
        guideLightGameObject.SetActive(true);
    }

    private void Product_OnProductTaken(object sender, System.EventArgs e)
    {
        guideLightGameObject.SetActive(false);
    }

    private void Product_OnProductBounced(object sender, System.EventArgs e)
    {
        guideLightGameObject.SetActive(true);
    }

    private void Product_OnProductPocketed(object sender, System.EventArgs e)
    {
        gameObject.SetActive(!product.IsInPocket());
    }
}
