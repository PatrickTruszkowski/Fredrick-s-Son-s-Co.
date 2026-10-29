using UnityEngine;

public class TrashAudioSource : MonoBehaviour
{
    [SerializeField] private Trash trash = null;

    private AudioSource audioSource = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        trash.OnInteractedWith += Trash_OnInteractedWith;
    }

    private void Trash_OnInteractedWith(object sender, System.EventArgs e)
    {
        Audio.RandomizePitchAndVolume(audioSource);

        audioSource.PlayOneShot(audioSource.clip);
    }
}
