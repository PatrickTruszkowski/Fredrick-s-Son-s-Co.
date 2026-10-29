using UnityEngine;

[RequireComponent(typeof(AudioLowPassFilter))]
public class DynamicAudioSource : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float effectStrength = 10;

    private Transform listenerTransform = null;

    private AudioLowPassFilter audioLowPassFilter = null;

    private float maximumFilterValue = 15000f;
    private float maximumPlayerDistance = 10f;

    private void Awake()
    {
        audioLowPassFilter = GetComponent<AudioLowPassFilter>();
    }

    private void Start()
    {
        if (Player.LocalSingleton != null)
        {
            listenerTransform = Player.LocalSingleton.GetPlayerCameraTransform();
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (Player.LocalSingleton != null)
        {
            listenerTransform = Player.LocalSingleton.GetPlayerCameraTransform();
        }
    }

    private void Update()
    {
        if (listenerTransform != null)
        {
            float playerDistance = Vector3.Distance(transform.position, listenerTransform.position);
            float normalizedDistance = playerDistance / maximumPlayerDistance;

            audioLowPassFilter.cutoffFrequency = maximumFilterValue / (normalizedDistance * effectStrength);
        }
    }

    public void SetListenerTransform(Transform transform)
    {
        listenerTransform = transform;
    }
}
