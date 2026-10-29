using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetcodeUI : MonoBehaviour
{
    [SerializeField] private GameObject uiGameObject = null;
    [SerializeField] private Button hostButton = null;
    [SerializeField] private Button clientButton = null;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => 
        { 
            NetworkManager.Singleton.StartHost();

            Hide();
        });

        clientButton.onClick.AddListener(() => 
        { 
            NetworkManager.Singleton.StartClient();

            Hide();
        });
    }

    private void Hide()
    {
        uiGameObject.SetActive(false);
    }
}
