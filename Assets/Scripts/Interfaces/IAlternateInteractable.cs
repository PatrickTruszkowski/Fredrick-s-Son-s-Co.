using System;
using UnityEngine;

public interface IAlternateInteractable
{
    public event EventHandler OnAlternateInteract;

    public void AlternateInteract(Interaction interaction);

    public Transform GetTransform();
}
