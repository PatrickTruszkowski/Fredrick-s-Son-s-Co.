using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAudioSource : NetworkBehaviour
{
    [SerializeField] private List<AudioClip> exhaustedAudioClipList = null;
    [SerializeField] private List<AudioClip> damagedAudioClipList = null;
    [SerializeField] private HealthSystem playerHealthSystem;

    private AudioSource audioSource = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (Player.LocalSingleton != null)
        {
            Player.LocalSingleton.OnPlayerExhausted += LocalPlayer_OnPlayerExhausted;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }

        if (!IsOwner)
        {
            return;
        }

        playerHealthSystem.OnDamaged += PlayerHealthSystem_OnDamaged;
        playerHealthSystem.OnDeath += PlayerHealthSystem_OnDeath;
    }

    private void Player_OnAnyPlayerSpawned(object sender, System.EventArgs e)
    {
        if (Player.LocalSingleton != null)
        {
            Player.LocalSingleton.OnPlayerExhausted -= LocalPlayer_OnPlayerExhausted;
            Player.LocalSingleton.OnPlayerExhausted += LocalPlayer_OnPlayerExhausted;
        }
    }

    private void PlayerHealthSystem_OnDeath(object sender, System.EventArgs e)
    {
        Audio.RandomizePitchAndVolume(audioSource, 0.95f, 1.05f, 0.5f, 0.55f);

        AudioClip randomAudioClip = Audio.GetRandomAudioClip(exhaustedAudioClipList);

        audioSource.PlayOneShot(randomAudioClip);

        PlayOtherPlayerDeathAudioServerRpc(exhaustedAudioClipList.IndexOf(randomAudioClip));
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayOtherPlayerDeathAudioServerRpc(int audioClipIndex)
    {
        PlayOtherPlayerDeathAudioClientRpc(audioClipIndex);
    }

    [ClientRpc]
    private void PlayOtherPlayerDeathAudioClientRpc(int audioClipIndex)
    {
        if (IsOwner)
        {
            return;
        }

        Audio.RandomizePitchAndVolume(audioSource);
        audioSource.PlayOneShot(exhaustedAudioClipList[audioClipIndex]);
    }

    private void PlayerHealthSystem_OnDamaged(object sender, System.EventArgs e)
    {
        if (playerHealthSystem.IsDead())
        {
            return;
        }

        Audio.RandomizePitchAndVolume(audioSource, 0.95f, 1.05f, 0.5f, 0.55f);

        AudioClip randomAudioClip = Audio.GetRandomAudioClip(damagedAudioClipList);

        audioSource.PlayOneShot(randomAudioClip);

        PlayOtherPlayerDamagedAudioServerRpc(damagedAudioClipList.IndexOf(randomAudioClip));
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayOtherPlayerDamagedAudioServerRpc(int audioClipIndex)
    {
        PlayOtherPlayerDamagedAudioClientRpc(audioClipIndex);
    }

    [ClientRpc]
    private void PlayOtherPlayerDamagedAudioClientRpc(int audioClipIndex)
    {
        if (IsOwner)
        {
            return;
        }

        Audio.RandomizePitchAndVolume(audioSource);
        audioSource.PlayOneShot(exhaustedAudioClipList[audioClipIndex]);
    }

    private void LocalPlayer_OnPlayerExhausted(object sender, System.EventArgs e)
    {
        Audio.RandomizePitchAndVolume(audioSource, 0.95f, 1.05f, 0.5f, 0.55f);

        AudioClip randomAudioClip = Audio.GetRandomAudioClip(exhaustedAudioClipList);

        audioSource.PlayOneShot(randomAudioClip);

        PlayOtherPlayerExhaustedAudioServerRpc(exhaustedAudioClipList.IndexOf(randomAudioClip));
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayOtherPlayerExhaustedAudioServerRpc(int audioClipIndex)
    {
        PlayOtherPlayerExhaustedAudioClientRpc(audioClipIndex);
    }

    [ClientRpc]
    private void PlayOtherPlayerExhaustedAudioClientRpc(int audioClipIndex)
    {
        if (IsOwner)
        {
            return;
        }

        Audio.RandomizePitchAndVolume(audioSource, 0.95f, 1.05f, 0.5f, 0.55f);
        audioSource.PlayOneShot(exhaustedAudioClipList[audioClipIndex]);
    }
}
