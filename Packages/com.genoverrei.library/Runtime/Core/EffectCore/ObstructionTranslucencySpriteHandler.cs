namespace Genoverrei.Library.Core
{
    public class ObstructionTranslucencySpriteHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected List<Transform> TargetParentList = new();

        [SerializeField] protected Color32 TranslucencyColor = new(75, 75, 75, 90);

        [SerializeField] protected float LerpDuration = 0.5f;

        protected Dictionary<int, SpriteRenderer> SpriteRenderersDict = new();
        protected Dictionary<int, Coroutine> ActiveCoroutinesDict = new();

        protected virtual void Awake()
        {
            Setup();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=green>[Enter]</color> {other.name}");
#endif
            StartColorLerp(other, TranslucencyColor, LerpDuration);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
#if UNITY_EDITOR
            Debug.Log($"<color=red>[Exit]</color> {other.name}");
#endif
            StartColorLerp(other, Color.white, LerpDuration);
        }

        protected virtual void Setup()
        {
            foreach (var parent in TargetParentList)
            {
                foreach (Transform children in parent)
                {
                    if (!children.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) continue;

                    int objectID = spriteRenderer.gameObject.GetInstanceID();

                    if (SpriteRenderersDict.ContainsKey(objectID)) continue;

                    SpriteRenderersDict.Add(objectID, spriteRenderer);
#if UNITY_EDITOR
                    Debug.Log($"<color=cyan>[Setup]</color> Add {spriteRenderer.name}");
#endif
                }
            }
        }

        protected virtual void StartColorLerp(Collider2D other, Color32 targetColor, float duration)
        {
            int objectID = other.gameObject.GetInstanceID();

            if (!SpriteRenderersDict.TryGetValue(objectID, out var spriteRenderer)) return;

            if (ActiveCoroutinesDict.TryGetValue(objectID, out var currentCoroutine) && currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            ActiveCoroutinesDict[objectID] = StartCoroutine(ColorLerpRoutine(objectID, spriteRenderer, targetColor, duration));
        }

        private IEnumerator ColorLerpRoutine(int objectID, SpriteRenderer spriteRenderer, Color targetColor, float duration)
        {
            float time = 0;
            Color startColor = spriteRenderer.color;

            while (time < duration)
            {
                if (spriteRenderer == null) yield break;

                spriteRenderer.color = Color.Lerp(startColor, targetColor, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = targetColor;
            ActiveCoroutinesDict[objectID] = null;
        }
    }
}