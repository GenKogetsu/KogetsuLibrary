using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    public class StatsController : MonoBehaviour
    {
        [Header("Stats Data")]
        [Required]
        [SerializeField] protected BaseStatsDataSO StatsData;

        [SerializeField] protected float CurrentMoveSpeed;

        [SerializeField] protected float CurrectJumpForce;

        public float GetBaseMoveSpeed() => StatsData != null ? StatsData.BaseMoveSpeed : 0f;
        public float GetBaseJumpForce() => StatsData != null ? StatsData.BaseJumpForce : 0f;
        public float GetMoveSpeed() => CurrentMoveSpeed;
        public float GetJumpForce() => CurrectJumpForce;
        public float GetMoveSpeedPercentage() => GetBaseMoveSpeed() > 0 ? CurrentMoveSpeed / GetBaseMoveSpeed() : 0f;

        private void Setup()
        {
            CurrentMoveSpeed = GetBaseMoveSpeed();
            CurrectJumpForce = GetBaseJumpForce();
        }

        private void Start()
        {
            Setup();
        }
    }
}
