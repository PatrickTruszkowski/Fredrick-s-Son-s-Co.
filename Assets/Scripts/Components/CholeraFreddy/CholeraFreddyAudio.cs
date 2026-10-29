using UnityEngine;

public class CholeraFreddyAudio : MonoBehaviour
{
    [SerializeField] private CholeraFreddy choleraFreddy;
    [SerializeField] private AudioClip noseBoopAudioClip = null;
    [SerializeField] private AudioClip winSongAudioClip = null;

    private AudioSource audioSource = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        choleraFreddy.OnNoseBoop += CholeraFreddy_OnNoseBoop;
        choleraFreddy.OnWin += CholeraFreddy_OnWin;
    }

    private void CholeraFreddy_OnWin(object sender, System.EventArgs e)
    {
        audioSource.clip = winSongAudioClip;

        audioSource.Play();
    }

    private void CholeraFreddy_OnNoseBoop(object sender, System.EventArgs e)
    {
        if (audioSource.clip == winSongAudioClip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = noseBoopAudioClip;

        audioSource.Play();
    }
}
