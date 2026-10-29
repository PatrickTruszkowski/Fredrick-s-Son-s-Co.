using UnityEngine;

public class DevelopmentBuildManager : MonoBehaviour
{
    public static DevelopmentBuildManager Singleton { get; private set; }

    [Header("External References")]
    [SerializeField] private Transform moonPivotTransform;

    private const float FULL_NIGHT_SECONDS = 720f;

    private float timeSeconds = 0f;

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if (timeSeconds > FULL_NIGHT_SECONDS) return;

        timeSeconds += Time.deltaTime;

        moonPivotTransform.rotation = Quaternion.Euler(new Vector3(-180f + (180f * timeSeconds / FULL_NIGHT_SECONDS), 0f, 0f));
    }
}
