using UnityEngine;
using UnityEngine.Rendering;

public class LocalPostProccessVolume : MonoBehaviour
{
    public static LocalPostProccessVolume LocalSingleton { get; private set; }

    public Volume volume { get; private set; }

    private void Awake()
    {
        LocalSingleton = this;

        volume = GetComponent<Volume>();
    }
}
