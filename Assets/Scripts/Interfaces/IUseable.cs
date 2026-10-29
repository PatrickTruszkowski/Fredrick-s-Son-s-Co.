using System;
using UnityEngine;

public interface IUseable
{
    public event EventHandler OnUsed;

    public void Use();

    public bool IsProductOn();

    public Transform GetTransform();
}
