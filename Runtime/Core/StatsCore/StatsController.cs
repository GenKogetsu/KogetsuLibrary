namespace Genoverrei.Library.DesignPatternCore
{
    public class StatsController : MonoBehaviour, IMoveStatsProvider
    {
        [Required]
        [SerializeField] private BaseStatsDataSO _statsData;

        public float GetMoveSpeed() => _statsData != null ? _statsData.BaseMoveSpeed : 0f;
        public float GetJumpForce() => _statsData != null ? _statsData.BaseJumpForce : 0f;
    }
}
