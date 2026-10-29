using System;
using UnityEngine;

public class Shotgun : Product, IUseable
{
    public event EventHandler OnUsed;

    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private Transform bulletShellPrefab;
    [SerializeField] private Transform[] bulletSpawnPointArray;
    [SerializeField] private Transform bulletShellSpawnPoint;

    public bool IsProductOn()
    {
        return false;
    }

    public void Use()
    {
        foreach (Transform bulletSpawnPoint in bulletSpawnPointArray)
        {
            Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }

        Transform bulletShellTransform = Instantiate(bulletShellPrefab, bulletShellSpawnPoint.position, bulletShellSpawnPoint.rotation);
        ObjectPoolManager.Singleton.AddPiece(bulletShellTransform.gameObject);

        Rigidbody bulletShellRigidbody = bulletShellTransform.GetComponent<Rigidbody>();

        Vector3 playerVelocity = GetInteraction().GetComponent<CharacterController>().velocity;

        bulletShellRigidbody.angularVelocity = new Vector3(UnityEngine.Random.Range(1f, 4f), UnityEngine.Random.Range(5f, 10f), UnityEngine.Random.Range(1f, 2f));
        bulletShellRigidbody.velocity = playerVelocity + (-transform.right + transform.up).normalized * 0.5f;

        OnUsed?.Invoke(this, EventArgs.Empty);
    }
}
