using System;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class ClickMoveAbility2D : BaseMoveAbility2D
    {
        [Header("Click Move Settings")]
        [SerializeField] protected LayerMask GroundLayer;
        [SerializeField] protected LayerMask SolidLayer;

        [Tooltip("ระยะเผื่อก่อนชนเป้าหมายหรือกำแพง")]
        [Range(0.05f, 0.5f)]
        [SerializeField] protected float StoppingDistance = 0.1f;

        [Header("Debug")]
        [SerializeField] protected bool ShowDebugRay = true;
        [Tooltip("ปรับสเกลของเส้น Debug ได้อิสระ (ปกติ = 1)")]
        [SerializeField] protected float DebugShapeScale = 1f;

        protected Vector3 TargetPosition;
        protected bool HasTarget;
        protected Camera MainCamera;
        protected RaycastHit2D Hit;
        protected Collider2D Collider;

        protected override void OnEnter()
        {
            MainCamera = Camera.main;
            if (Context?.Transform != null && Collider == null)
                Collider = Context.Transform.GetComponent<Collider2D>();

            if (Context.InputObserverChannel != null)
                Context.InputObserverChannel.OnLeftClickChannel += SetDestination;
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel != null)
                Context.InputObserverChannel.OnLeftClickChannel -= SetDestination;
        }

        protected virtual void SetDestination(ClickData clickData)
        {
            if (MainCamera == null) return;

            Vector3 worldPos = MainCamera.ScreenToWorldPoint(clickData.ClickPosition);
            worldPos.z = 0;

            RaycastHit2D hitInfo = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, GroundLayer);
            TargetPosition = hitInfo.collider != null ? hitInfo.point : (Vector2)worldPos;
            HasTarget = true;
        }

        protected override void OnFixedUpdate()
        {
            if (!HasTarget || Context?.Stats == null || Context.Rb2 == null || Collider == null) return;

            Vector2 center = Collider.bounds.center;
            float radius = Mathf.Max(Collider.bounds.extents.x, Collider.bounds.extents.y);
            float totalStop = radius + StoppingDistance;

            Vector2 target2D = TargetPosition;
            Vector2 dir = (target2D - center).normalized;
            float dist = Vector2.Distance(center, target2D);

            Hit = Physics2D.CircleCast(center, radius, dir, StoppingDistance, SolidLayer);

            if (dist <= totalStop || Hit.collider != null)
            {
                HasTarget = false;
                Context.Rb2.linearVelocity = Vector2.zero;
                return;
            }

            Context.Rb2.linearVelocity = dir * Context.Stats.GetMoveSpeed();
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform transform)
        {
            if (Application.isPlaying || !ShowDebugRay) return;

            if (!transform.TryGetComponent(out Collider2D col)) return;

            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale * DebugShapeScale);

            switch (col)
            {
                case BoxCollider2D box:
                    Gizmos.DrawWireCube(box.offset, box.size);
                    break;
                case CircleCollider2D circle:
                    Gizmos.DrawWireSphere(circle.offset, circle.radius);
                    break;
                case CapsuleCollider2D capsule:
                    Gizmos.DrawWireCube(capsule.offset, capsule.size);
                    break;
                default:
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                    break;
            }

            Gizmos.matrix = Matrix4x4.identity;
            float r = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(col.bounds.center, col.bounds.center + transform.up * (r + StoppingDistance));
        }
#endif
    }
}
