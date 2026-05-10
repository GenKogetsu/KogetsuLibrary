using System;

namespace Genoverrei.Library.Core;

/// <summary>
/// Observer channel สำหรับ input เพิ่มเติมของ Player:
/// Roll, Flashlight toggle, Shoot, Mouse position
/// ใช้คู่กับ BasicMovementInputObserverSO ของ library (move/jump)
/// </summary>
[CreateAssetMenu(fileName = "PlayerInputObserver", menuName = "Game/Observer/PlayerInputObserver")]
public class PlayerInputObserverSO : ScriptableObject
{
    public Action              OnRollChannel;
    public Action              OnFlashlightToggleChannel;
    public Action              OnShootChannel;
    public Action<Vector2>     OnMousePositionChannel;

    public void SendRollSignal()                      => OnRollChannel?.Invoke();
    public void SendFlashlightToggleSignal()          => OnFlashlightToggleChannel?.Invoke();
    public void SendShootSignal()                     => OnShootChannel?.Invoke();
    public void SendMousePositionSignal(Vector2 pos)  => OnMousePositionChannel?.Invoke(pos);
}
