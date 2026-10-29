using UnityEngine;

[CreateAssetMenu()]
public class ProductAsset : ScriptableObject
{
    [Header("Sounds")]
    public AudioClip dropSound = null;
    public AudioClip bounceSound = null;
    public AudioClip takeSound = null;
    public AudioClip useSound = null;
    public float minimumUsePitch = 1f;
    public float maximumUsePitch = 1f;
    public float minimumDropPitch = 1f;
    public float maximumDropPitch = 1f;

    [Header("Storage")]
    public int size = 1;
    public Transform hotbarImage = null;

    [Header("Physics")]
    public float bounceFactor = 0.1f;
    public float mass = 1f;

    [Header("Offsets")]
    public float normalYRotation = 0f;

    [Header("Other")]
    public string displayName = null;
}
