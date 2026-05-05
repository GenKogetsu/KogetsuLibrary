namespace Genoverrei.Library.Core
{
    [RequireComponent(typeof(Collider2D))]
    public class ObstructionTranslucencyAnimatorHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] protected List<Transform> TargetParentList = new();

        protected Dictionary<int, Animator> AnimatorsDict = new();

        protected virtual void Awake()
        {
            Setup();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
#if UNITY_EDITOR
            Debug.Log($"Enter {other.name}");
#endif
            SetBlendingMode(other, true);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
#if UNITY_EDITOR
            Debug.Log($"Exit {other.name}");
#endif
            SetBlendingMode(other, false);
        }

        protected virtual void Setup()
        {
            foreach (var childrenArray in TargetParentList)
            {
                foreach (Transform children in childrenArray)
                {
                    if (!children.TryGetComponent<Animator>(out var animator)) continue;

                    int objectID = animator.gameObject.GetInstanceID();

                    if (AnimatorsDict.ContainsKey(objectID)) continue;

                    AnimatorsDict.Add(objectID, animator);
#if UNITY_EDITOR
                    Debug.Log($"Add {animator.name}");
#endif
                }
            }
        }

        protected virtual void SetBlendingMode(Collider2D other, bool mode)
        {
            int objectID = other.gameObject.GetInstanceID();

            if (!AnimatorsDict.TryGetValue(objectID, out var animator)) return;

            animator.SetBool("IsHitPlayer", mode);
        }
    }
}