using System;

namespace Genoverrei.Library.DesignPatternCore
{

    [CreateAssetMenu(fileName = "MoveInputChannel", menuName = "GenoverreiLibrary/DesignPattern/Observer/InputObserverChannel")]
    public class InputObserverChannelSO : ScriptableObject
    {
        public Action<Vector3> OnMoveChannel;
        public Action OnInteractionChannel;
        public Action OnJumpChannel;

        public void SendMoveSignal(Vector3 value) 
        { 
            if (OnMoveChannel == null) return;

            OnMoveChannel?.Invoke(value);
        } 
        public void SendInteractionSignal() => OnInteractionChannel?.Invoke();
        public void SendJumpSignal() => OnJumpChannel?.Invoke();
    }
}