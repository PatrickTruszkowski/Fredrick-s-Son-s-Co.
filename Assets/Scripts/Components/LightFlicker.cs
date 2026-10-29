using SpaceDoktor.Utils;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private float stepAmount = 0.1f;
    [SerializeField] private float lightSmoothing = 16f;
    [SerializeField] private float minimumIntensity = 0.6f;
    [SerializeField] private float maximumIntensity = 1f;

    private Light lightSource = null;
    private float step = 0;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
    }

    private void Start()
    {
        Building.Singleton.OnBuildingPowerLost += Building_OnBuildingPowerLost;
    }

    private void Update()
    {
        if (step > stepAmount)
        {
            step = 0;
        }

        if (step % stepAmount == 0)
        {
            lightSource.intensity = Smoothing.Exponential(lightSource.intensity, Random.Range(minimumIntensity, maximumIntensity), lightSmoothing, Time.deltaTime);
        }

        step += Time.deltaTime;
    }

    private void Building_OnBuildingPowerLost(object sender, System.EventArgs e)
    {
        minimumIntensity = 0f;
        maximumIntensity = 0.25f; 
    }
}
