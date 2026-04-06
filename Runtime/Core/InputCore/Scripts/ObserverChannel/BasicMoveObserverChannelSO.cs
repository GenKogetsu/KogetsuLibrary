using Genoverrei.Library.Core;
using System;

namespace Genoverrei.Library.DesignPatternCore
{

    [CreateAssetMenu(fileName = "BasicObserverChannel", menuName = "GenoverreiLibrary/DesignPattern/Observer/BasicObserverChannel")]
    public class BasicObserverChannelSO : ScriptableObject
    {
        public Action OnPressAnyKeyChannel;

        public Action<Vector3> OnMoveChannel;

        public Action OnInteractionChannel;

        public Action OnJumpChannel;

        public Action<ClickData> OnLeftClickChannel;
        public Action<ClickData> OnMiddleClickChannel;
        public Action<ClickData> OnRightClickChannel;



        public void SendMoveSignal(Vector3 value) => OnMoveChannel?.Invoke(value);
        
        public void SendInteractionSignal() => OnInteractionChannel?.Invoke();

        public void SendJumpSignal() => OnJumpChannel?.Invoke();

        public void SendLeftClickSignal(ClickData clickData) => OnLeftClickChannel?.Invoke(clickData);
        public void SendMiddleClickSignal(ClickData clickData) => OnMiddleClickChannel?.Invoke(clickData);
        public void SendRightClickSignal(ClickData clickData) => OnRightClickChannel?.Invoke(clickData);

    }
}