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

        // ─── Color Lerp — Single Renderer ──────────────────────────────────
        [Header("Color Lerp — Single Renderer")]
        [Tooltip("ใช้เมื่อตัวละครมี SpriteRenderer เดียว (Spritesheet mode)")]
        [SerializeField] protected SpriteRenderer targetRenderer;

        // ─── Color Lerp — Multi Renderer ───────────────────────────────────
        [Header("Color Lerp — Multi Renderer (Cutout2D Rig)")]
        [Tooltip("ถ้าใส่ array นี้จะ override targetRenderer — " +
                 "ใช้กับ 2D rig ที่มีหลาย SpriteRenderer\n" +
                 "ปล่อยว่างได้ถ้าใช้ Single Renderer หรือใช้ RiggedHurtFlash แทน")]
        [SerializeField] protected SpriteRenderer[] targetRenderers;

        // ─── Shared Lerp Settings ──────────────────────────────────────────
        [Header("Color Lerp Settings")]
        [SerializeField] protected Color startColor = Color.white;
        [SerializeField] protected Color endColor   = Color.red;
        [SerializeField] protected float lerpSpeed  = 10f;
        [SerializeField] protected int   lerpCount  = 3;

        private Coroutine _lerpCoroutine;

        // ─── Stats ─────────────────────────────────────────────────────────
        public float GetBaseMoveSpeed()      => StatsData != null ? StatsData.BaseMoveSpeed  : 0f;
        public float GetBaseJumpForce()      => StatsData != null ? StatsData.BaseJumpForce  : 0f;
        public float GetMoveSpeed()          => CurrentMoveSpeed;
        public float GetJumpForce()          => CurrectJumpForce;
        public float GetMoveSpeedPercentage() =>
            GetBaseMoveSpeed() > 0 ? CurrentMoveSpeed / GetBaseMoveSpeed() : 0f;

        protected virtual void Setup()
        {
            CurrentMoveSpeed = GetBaseMoveSpeed();
            CurrectJumpForce = GetBaseJumpForce();

            TotalHp   = MaxHp;
            CurrentHp = MaxHp;
        }

        protected virtual void Start() => Setup();

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

        // ─── HP ────────────────────────────────────────────────────────────
        public void TakeDamage(float damage)
        {
            CurrentHp = Mathf.Clamp(CurrentHp - damage, MinHp, MaxHp);
            TriggerColorLerp();
        }

        public void Heal(float healAmount) =>
            CurrentHp = Mathf.Clamp(CurrentHp + healAmount, MinHp, MaxHp);

        public void SetCurrentHp(float hp) => CurrentHp = Mathf.Clamp(hp, MinHp, MaxHp);
        public void ResetHp()              => CurrentHp = MaxHp;

        public float GetCurrentHp() => CurrentHp;
        public float GetTotalHp()   => TotalHp;
        public float GetMaxHp()     => MaxHp;
        public float GetMinHp()     => MinHp;

        protected void OnTakeDamage(DealDamageEvent Data)
        {
            TakeDamage(Data.Damage);
            if (EventBus.Instance)
                EventBus.Instance.Publish(new TakeDamageEvent(Data.Damage, CurrentHp));
        }

        // ─── Color Lerp ────────────────────────────────────────────────────
        protected void TriggerColorLerp()
        {
            bool hasMulti  = targetRenderers != null && targetRenderers.Length > 0;
            bool hasSingle = targetRenderer != null;
            if (!hasMulti && !hasSingle) return;

            if (_lerpCoroutine != null) StopCoroutine(_lerpCoroutine);
            _lerpCoroutine = StartCoroutine(hasMulti ? LerpAllColorRoutine() : LerpColorRoutine());
        }

        // Single renderer
        private IEnumerator LerpColorRoutine()
        {
            for (int i = 0; i < lerpCount; i++)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime * lerpSpeed)
                {
                    targetRenderer.color = Color.Lerp(startColor, endColor, t);
                    yield return null;
                }
                for (float t = 0f; t < 1f; t += Time.deltaTime * lerpSpeed)
                {
                    targetRenderer.color = Color.Lerp(endColor, startColor, t);
                    yield return null;
                }
            }
            targetRenderer.color = startColor;
        }

        // Multi renderer — flashes every SpriteRenderer in the array
        private IEnumerator LerpAllColorRoutine()
        {
            for (int i = 0; i < lerpCount; i++)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime * lerpSpeed)
                {
                    Color c = Color.Lerp(startColor, endColor, Mathf.Clamp01(t));
                    SetAllColors(c);
                    yield return null;
                }
                for (float t = 0f; t < 1f; t += Time.deltaTime * lerpSpeed)
                {
                    Color c = Color.Lerp(endColor, startColor, Mathf.Clamp01(t));
                    SetAllColors(c);
                    yield return null;
                }
            }
            SetAllColors(startColor);
        }

        private void SetAllColors(Color color)
        {
            foreach (var sr in targetRenderers)
                if (sr != null) sr.color = color;
        }
    }
}
