using System;

namespace Genoverrei.Library.Core
{
    [CreateAssetMenu(fileName = "DirectionModeObserver", menuName = "GenoverreiLibrary/DesignPattern/Observer/DirectionModeObserver")]
    public class DirectionModeObserverSO : ScriptableObject
    {
        public Action<DirectionMode> OnDirectionModeObserver;

        public void SendDirectionModeSignal(DirectionMode directionMode)
        {
            OnDirectionModeObserver?.Invoke(directionMode);
        }

        public byte ConvertDirectionModeToByte(DirectionMode directionMode) => directionMode switch
        {
            DirectionMode.OneDiraction => 1,
            DirectionMode.TwoDiraction => 2,
            DirectionMode.FourDiraction => 4,
            DirectionMode.EightDiraction => 8,
            _ => 0
        };
    }
}
