using UnityEngine;

public class ShotgunAnimator : MonoBehaviour
{
    [SerializeField] private Shotgun shotgun = null;

    private const string IS_HELD = "IsHeld";

    private Animator animator = null;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        shotgun.OnUsed += Shotgun_OnUsed;
        shotgun.OnProductTaken += Shotgun_OnProductTaken;
        shotgun.OnProductHitGround += Shotgun_OnProductHitGround;
    }

    private void Shotgun_OnUsed(object sender, System.EventArgs e)
    {
        PlayerCameraShaker.LocalSingleton.Shake(transform.position, ShakeIntensity.Heavy, 1f);
    }

    private void Shotgun_OnProductTaken(object sender, System.EventArgs e)
    {
        animator.SetBool(IS_HELD, true);
    }

    private void Shotgun_OnProductHitGround(object sender, System.EventArgs e)
    {
        animator.SetBool(IS_HELD, false);
    }
}
