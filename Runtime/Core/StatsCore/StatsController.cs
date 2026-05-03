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

        [Header("Hp Settings")]
        [SerializeField] protected float CurrentHp;
        [SerializeField] protected float TotalHp;
        [SerializeField] protected float MaxHp;
        [SerializeField] protected float MinHp;

        [Header("Color Lerp Settings")]
        [SerializeField] protected SpriteRenderer targetRenderer;
        [SerializeField] protected Color startColor = Color.white;
        [SerializeField] protected Color endColor = Color.red;
        [SerializeField] protected float lerpSpeed = 10f;
        [SerializeField] protected int lerpCount = 3;

        private Coroutine _lerpCoroutine;

        public float GetBaseMoveSpeed() => StatsData != null ? StatsData.BaseMoveSpeed : 0f;
        public float GetBaseJumpForce() => StatsData != null ? StatsData.BaseJumpForce : 0f;
        public float GetMoveSpeed() => CurrentMoveSpeed;
        public float GetJumpForce() => CurrectJumpForce;
        public float GetMoveSpeedPercentage() => GetBaseMoveSpeed() > 0 ? CurrentMoveSpeed / GetBaseMoveSpeed() : 0f;

        protected virtual void Setup()
        {
            CurrentMoveSpeed = GetBaseMoveSpeed();
            CurrectJumpForce = GetBaseJumpForce();

            TotalHp = MaxHp;
            CurrentHp = MaxHp;
        }

        protected virtual void Start()
        {
            Setup();
        }

        protected void OnEnable()
        {
            ResetHp();

            if (!EventBus.Instance) return;

            EventBus.Instance.Subscribe<DealDamageEvent>(OnTakeDamage);
        }

        protected void OnDisable()
        {
            if (!EventBus.Instance) return;
            EventBus.Instance.Unsubscribe<DealDamageEvent>(OnTakeDamage);
        }

        public void TakeDamage(float damage)
        {
            CurrentHp = Mathf.Clamp(CurrentHp - damage, MinHp, MaxHp);
            TriggerColorLerp();
        }

        public void Heal(float healAmount)
        {
            CurrentHp = Mathf.Clamp(CurrentHp + healAmount, MinHp, MaxHp);
        }

        public void SetCurrentHp(float hp) => CurrentHp = Mathf.Clamp(hp, MinHp, MaxHp);

        public void ResetHp() => CurrentHp = MaxHp;

        public float GetCurrentHp() => CurrentHp;
        public float GetTotalHp() => TotalHp;
        public float GetMaxHp() => MaxHp;
        public float GetMinHp() => MinHp;

        protected void OnTakeDamage(DealDamageEvent Data)
        {
            TakeDamage(Data.Damage);

            if (EventBus.Instance) EventBus.Instance.Publish(new TakeDamageEvent(Data.Damage, CurrentHp));
        }

        protected void TriggerColorLerp()
        {
            if (targetRenderer == null) return;

            if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
            _lerpCoroutine = StartCoroutine(LerpColorRoutine());
        }

        private IEnumerator LerpColorRoutine()
        {
            for (int i = 0; i < lerpCount; i++)
            {
                float t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime * lerpSpeed;
                    targetRenderer.color = Color.Lerp(startColor, endColor, t);
                    yield return null;
                }

                t = 0;
                while (t < 1f)
                {
                    t += Time.deltaTime * lerpSpeed;
                    targetRenderer.color = Color.Lerp(endColor, startColor, t);
                    yield return null;
                }
            }

            targetRenderer.color = startColor;
        }
    }
}
