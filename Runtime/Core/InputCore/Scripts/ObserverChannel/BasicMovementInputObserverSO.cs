using Kogetsu.Library.Core;
using System;

namespace Kogetsu.Library.DesignPatternCore
{

    [CreateAssetMenu(fileName = "BasicMovementInputObserver", menuName = "KogetsuLibrary/DesignPattern/Observer/BasicMovementInputObserver")]
    public class BasicMovementInputObserverSO : ScriptableObject
    {
        // ─── General ───────────────────────────────────────────────────────
        public Action OnPressAnyKeyChannel;

        // ─── Movement ──────────────────────────────────────────────────────
        public Action<Vector3> OnMoveChannel;

        public Action OnJumpChannel;
        public Action OnJumpReleasedChannel;

        // ─── Interaction ───────────────────────────────────────────────────
        public Action OnInteractionChannel;

        // ─── Mouse / Click ─────────────────────────────────────────────────
        public Action<ClickData> OnLeftClickChannel;
        public Action<ClickData> OnMiddleClickChannel;
        public Action<ClickData> OnRightClickChannel;

        public Action<Vector2> OnMousePositionChannel;

        // ─── Player Actions ────────────────────────────────────────────────
        public Action OnRollChannel;
        public Action OnFlashlightToggleChannel;
        public Action OnShootChannel;

        // ─── Send helpers ──────────────────────────────────────────────────
        public void SendMoveSignal(Vector3 value)          => OnMoveChannel?.Invoke(value);

        public void SendJumpSignal()                       => OnJumpChannel?.Invoke();
        public void SendJumpReleasedSignal()               => OnJumpReleasedChannel?.Invoke();

        public void SendInteractionSignal()                => OnInteractionChannel?.Invoke();

        public void SendLeftClickSignal(ClickData data)    => OnLeftClickChannel?.Invoke(data);
        public void SendMiddleClickSignal(ClickData data)  => OnMiddleClickChannel?.Invoke(data);
        public void SendRightClickSignal(ClickData data)   => OnRightClickChannel?.Invoke(data);

        public void SendMousePositionSignal(Vector2 pos)   => OnMousePositionChannel?.Invoke(pos);

        public void SendRollSignal()                       => OnRollChannel?.Invoke();
        public void SendFlashlightToggleSignal()           => OnFlashlightToggleChannel?.Invoke();
        public void SendShootSignal()                      => OnShootChannel?.Invoke();
    }
}