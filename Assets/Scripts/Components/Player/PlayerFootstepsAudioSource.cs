using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerFootstepsAudioSource : NetworkBehaviour
{
    [SerializeField] private PlayerFootsteps footsteps = null;
    [SerializeField] private List<AudioClip> footstepsAudioClipList = null;
    [SerializeField] private List<AudioClip> initialSlideAudioClipList = null;
    [SerializeField] private AudioClip slideAudioClip = null;

    private AudioSource audioSource = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        Player.LocalSingleton.OnPlayerStoppedSliding += LocalPlayer_OnPlayerStoppedSliding;

        footsteps.OnPlayWalkingFootstep += Footsteps_OnPlayWalkingFootstep;
        footsteps.OnPlayRunningFootstep += Footsteps_OnPlayRunningFootstep;
        footsteps.OnPlaySlideSound += Footsteps_OnPlaySlideSound;
    }

    private void LocalPlayer_OnPlayerStoppedSliding(object sender, System.EventArgs e)
    {
        audioSource.Stop();

        OtherPlayerStoppedSlidingServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OtherPlayerStoppedSlidingServerRpc()
    {
        OtherPlayerStoppedSlidingClientRpc();
    }

    [ClientRpc]
    private void OtherPlayerStoppedSlidingClientRpc()
    {
        if (IsOwner)
        {
            return;
        }

        audioSource.Stop();
    }

    private void Footsteps_OnPlayWalkingFootstep(object sender, System.EventArgs e)
    {
        Audio.RandomizePitchAndVolume(audioSource, 0.95f, 1.05f, 0.3f, 0.35f);
        audioSource.PlayOneShot(Audio.GetRandomAudioClip(footstepsAudioClipList));
    }

    private void Footsteps_OnPlayRunningFootstep(object sender, System.EventArgs e)
    {
        Audio.RandomizePitchAndVolume(audioSource, 0.95f, 1.05f, 1f, 1f);
        audioSource.PlayOneShot(Audio.GetRandomAudioClip(footstepsAudioClipList));
    }

    private void Footsteps_OnPlaySlideSound(object sender, System.EventArgs e)
    {
        Audio.RandomizePitchAndVolume(audioSource);
        audioSource.PlayOneShot(Audio.GetRandomAudioClip(initialSlideAudioClipList));

        Audio.RandomizePitchAndVolume(audioSource);
        audioSource.PlayOneShot(slideAudioClip);
    }
}
