using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public event EventHandler OnUse;
    public event EventHandler OnDropPerformed;
    public event EventHandler OnDropCanceled;
    public event EventHandler OnInteract;
    public event EventHandler OnAlternateInteract;
    public event EventHandler OnCrouch;
    public event EventHandler OnEmote0;
    public event EventHandler OnEmote1;
    public event EventHandler OnTest;

    public static InputManager Singleton { get; private set; }

    private FPCInputActions inputActions = null;
    private InputDevice currentInputDevice = null;

    private void Awake()
    {
        Singleton = this;

        inputActions = new FPCInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.FPC.Interact.performed += Interact_performed;
        inputActions.FPC.AlternateInteract.performed += AlternateInteract_performed;
        inputActions.FPC.Drop.performed += Drop_performed;
        inputActions.FPC.Drop.canceled += Drop_canceled;
        inputActions.FPC.Use.performed += Use_performed;
        inputActions.FPC.Crouch.performed += Crouch_performed;
        inputActions.FPC.Emote0.performed += Emote0_performed;
        inputActions.FPC.Emote1.performed += Emote1_performed;
        inputActions.FPC.Test.performed += Test_performed;
    }

    private void OnDisable()
    {
        inputActions.Disable();

        inputActions.FPC.Interact.performed -= Interact_performed;
        inputActions.FPC.AlternateInteract.performed -= AlternateInteract_performed;
        inputActions.FPC.Drop.performed -= Drop_performed;
        inputActions.FPC.Drop.canceled -= Drop_canceled;
        inputActions.FPC.Use.performed -= Use_performed;
        inputActions.FPC.Crouch.performed -= Crouch_performed;
        inputActions.FPC.Emote0.performed -= Emote0_performed;
        inputActions.FPC.Emote1.performed -= Emote1_performed;
        inputActions.FPC.Test.performed -= Test_performed;
    }

    private void Test_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnTest?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Crouch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnCrouch?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Emote0_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnEmote0?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Emote1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnEmote1?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Use_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnUse?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Drop_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnDropPerformed?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Drop_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnDropCanceled?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void AlternateInteract_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnAlternateInteract?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteract?.Invoke(this, EventArgs.Empty);

        SetCurrentInputDevice(obj.control.device);
    }

    private void SetCurrentInputDevice(InputDevice inputDevice)
    {
        currentInputDevice = inputDevice;
    }

    public Vector2 NormalizedInputVector()
    {
        if (inputActions.FPC.Move.activeControl != null)
        {
            SetCurrentInputDevice(inputActions.FPC.Move.activeControl.device);
        }

        return inputActions.FPC.Move.ReadValue<Vector2>();
    }

    public Vector2 InvertedMouseDeltaVector()
    {
        if (inputActions.FPC.Look.activeControl != null)
        {
            SetCurrentInputDevice(inputActions.FPC.Look.activeControl.device);
        }

        return inputActions.FPC.Look.ReadValue<Vector2>();
    }

    public float GetMouseScroll()
    {
        return inputActions.FPC.Switch.ReadValue<float>();
    }

    public bool IsPressingRun()
    {
        if (inputActions.FPC.Run.activeControl != null)
        {
            SetCurrentInputDevice(inputActions.FPC.Run.activeControl.device);
        }

        return inputActions.FPC.Run.IsPressed();
    }

    public bool IsActiveDeviceController()
    {
        return currentInputDevice is Gamepad;
    }
}
