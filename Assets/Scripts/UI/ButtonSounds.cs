using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ButtonSounds : MonoBehaviour, IPointerEnterHandler
{
    private Button button = null;
    private AudioSource audioSource = null;

    private void Awake()
    {
        button = GetComponent<Button>();
        audioSource = GetComponent<AudioSource>();

        button.onClick.AddListener(() =>
        {
            audioSource.pitch = 1.4f;
            audioSource.PlayOneShot(audioSource.clip);
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(audioSource.clip);
    }
}
