using UnityEngine.InputSystem;

namespace Genoverrei.Library.Core;

/// <summary>
/// Extends BasicInputManager ของ library เพิ่ม callbacks:
/// Roll, Flashlight, Shoot, MousePosition
///
/// Setup ใน Unity:
/// 1. ใส่ Component นี้บน PlayerInput GameObject
/// 2. PlayerInput → Behavior: "Invoke Unity Events"
/// 3. Assign BasicObserverChannel + PlayerObserver
/// 4. Map actions ใน Input Actions asset ตาม README_SETUP
/// </summary>
public class PlayerInputHandler : BasicInputManager
{
    [Header("Player Observer")]
    [SerializeField] private PlayerInputObserverSO PlayerObserver;

    public void OnRollInput(InputAction.CallbackContext context)
    {
        if (!context.performed || PlayerObserver == null) return;
        PlayerObserver.SendRollSignal();
    }

    public void OnFlashlightInput(InputAction.CallbackContext context)
    {
        if (!context.performed || PlayerObserver == null) return;
        PlayerObserver.SendFlashlightToggleSignal();
    }

    public void OnShootInput(InputAction.CallbackContext context)
    {
        if (!context.performed || PlayerObserver == null) return;
        PlayerObserver.SendShootSignal();
    }

    /// <summary>
    /// Map action type: Value / Vector2
    /// ส่ง screen position ทุก frame ที่เมาส์ขยับ
    /// </summary>
    public void OnMousePositionInput(InputAction.CallbackContext context)
    {
        if (PlayerObserver == null) return;
        PlayerObserver.SendMousePositionSignal(context.ReadValue<Vector2>());
    }
}
