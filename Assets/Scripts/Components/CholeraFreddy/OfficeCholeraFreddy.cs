using UnityEngine;

public class OfficeCholeraFreddy : MonoBehaviour
{
    [SerializeField] private CholeraFreddy choleraFreddy = null;
    [SerializeField] private GameObject officeCholeraFreddyGameObject = null;

    private Collider officeCholeraFreddyCollider = null;

    private void Awake()
    {
        officeCholeraFreddyCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        choleraFreddy.OnWin += CholeraFreddy_OnWin;
    }

    private void CholeraFreddy_OnWin(object sender, System.EventArgs e)
    {
        officeCholeraFreddyGameObject.SetActive(true);
        officeCholeraFreddyCollider.enabled = true;
    }
}
