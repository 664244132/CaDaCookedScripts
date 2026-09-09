using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }
    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;
    public event EventHandler OnDashAction;

    private PlayerInputActions playerInputActions;
    private InputAction dashAction;

    private void Awake()
    {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;

        // ตั้งค่าปุ่ม Dash ด้วย New Input System (Spacebar บนคีย์บอร์ด, Button South / Right Shoulder บน Gamepad)
        dashAction = new InputAction("Dash", InputActionType.Button);
        dashAction.AddBinding("<Keyboard>/space");
        dashAction.AddBinding("<Gamepad>/buttonSouth");
        dashAction.AddBinding("<Gamepad>/rightShoulder");
        dashAction.performed += Dash_performed;
        dashAction.Enable();
    }

    private void OnDestroy()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Player.Interact.performed -= Interact_performed;
            playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
            playerInputActions.Player.Pause.performed -= Pause_performed;
            playerInputActions.Dispose();
        }

        if (dashAction != null)
        {
            dashAction.performed -= Dash_performed;
            dashAction.Disable();
            dashAction.Dispose();
        }
    }

    private void Dash_performed(InputAction.CallbackContext obj)
    {
        OnDashAction?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ตรวจสอบว่าปุ่ม Alternate (F บนคีย์บอร์ด หรือ West บน Gamepad) กำลังถูกกดค้างอยู่หรือไม่
    /// </summary>
    public bool IsInteractAlternatePressed()
    {
        if (playerInputActions == null) return false;
        return playerInputActions.Player.InteractAlternate.IsPressed();
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
    }
}

