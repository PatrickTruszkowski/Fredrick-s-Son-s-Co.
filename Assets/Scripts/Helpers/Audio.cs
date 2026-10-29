using System.Collections.Generic;
using UnityEngine;

public static class Audio
{
    public static AudioClip GetRandomAudioClip(List<AudioClip> audioClipList)
    {
        if (audioClipList == null)
        {
            Debug.LogWarning("audioClipArray is null.");

            return null;
        }

        if (audioClipList.Count == 1)
        {
            return audioClipList[0];
        }

        return audioClipList[Random.Range(0, audioClipList.Count)];
    }

    public static void RandomizePitchAndVolume(AudioSource audioSource, float minimumPitch = 0.95f, float maximumPitch = 1.05f, float minimumVolume = 0.9f, float maximumVolume = 1f)
    {
        audioSource.pitch = Random.Range(minimumPitch, maximumPitch);
        audioSource.volume = Random.Range(minimumVolume, maximumVolume);
    }
}
