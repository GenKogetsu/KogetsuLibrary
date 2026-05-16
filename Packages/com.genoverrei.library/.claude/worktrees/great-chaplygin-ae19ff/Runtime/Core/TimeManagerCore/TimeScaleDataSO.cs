namespace Genoverrei.Library.Core
{
    [CreateAssetMenu(fileName = "TimeScaleData", menuName = "GenoverreiLibrary/Core/TimeScaleData")]
    public class TimeScaleDataSO : ScriptableObject
    {
        public float BaseAcceleratorScale = 1.0f;
        public float BaseAccelerationMultiplier = 1.0f;
    }
}