using System;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public class ClickMoveAbility2D : BaseMoveAbility2D, IEnterState, IFixedUpdateState, IExitState
    {
        [Header("Click Move Settings")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _solidLayer;

        [Tooltip("ระยะเผื่อก่อนชนเป้าหมายหรือกำแพง")]
        [Range(0.5f, 0.05f), MinValue(0.05f)]
        [SerializeField] private float _stoppingDistance = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugRay = true;
        [Tooltip("ปรับสเกลของเส้น Debug ได้อิสระ (ปกติ = 1)")]
        [SerializeField] private float _debugShapeScale = 1f;

        private Vector3 _targetPosition;
        private bool _hasTarget;
        private Camera _mainCamera;
        private RaycastHit2D _hit;
        private Collider2D _collider;

        public void OnEnter()
        {
            _mainCamera = Camera.main;
            if (Context?.Transform != null && _collider == null) _collider = Context.Transform.GetComponent<Collider2D>();

            if (InputObserverChannel != null)
                InputObserverChannel.OnLeftClickChannel += SetDestination;
        }

        public void OnExit()
        {
            if (InputObserverChannel != null)
                InputObserverChannel.OnLeftClickChannel -= SetDestination;
        }

        public void SetDestination(ClickData clickData)
        {
            if (_mainCamera == null) return;

            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(clickData.ClickPosition);
            worldPos.z = 0;

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity, _groundLayer);

            _targetPosition = hit.collider != null ? hit.point : worldPos;
            _hasTarget = true;
        }

        public void OnFixedUpdate()
        {
            if (!_hasTarget || Context?.Stats == null || Context.Rigidbody == null || _collider == null) return;

            Vector2 centerPos = _collider.bounds.center;
            float radius = Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.y);
            float totalStoppingDistance = radius + _stoppingDistance;

            Vector2 targetPos2D = new(_targetPosition.x, _targetPosition.y);
            float distance = Vector2.Distance(centerPos, targetPos2D);

            Vector2 directionToTarget = (targetPos2D - centerPos).normalized;

            // เช็คว่าชนกำแพงด้วยความกว้างตัวละครหรือไม่
            _hit = Physics2D.CircleCast(centerPos, radius, directionToTarget, _stoppingDistance, _solidLayer);
            bool hitWall = _hit.collider != null;

            // ถ้าถึงเป้าหมาย หรือ "ชนกำแพง" ให้หยุดทันที
            if (distance <= totalStoppingDistance || hitWall)
            {
                _hasTarget = false;
                Context.Rigidbody.linearVelocity = Vector2.zero;
                return;
            }

            // เคลื่อนที่ตรงไป
            Context.Rigidbody.linearVelocity = directionToTarget * Context.Stats.GetMoveSpeed();
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform transform)
        {
            if (Application.isPlaying || !_showDebugRay) return;

            if (transform.TryGetComponent(out Collider2D col))
            {
                Gizmos.color = Color.cyan;

                // แมปรูปทรงและสเกล
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale * _debugShapeScale);

                // วาดรูปร่างเป๊ะๆ จาก Setting ของ Collider2D
                if (col is BoxCollider2D box)
                {
                    Gizmos.DrawWireCube(box.offset, box.size);
                }
                else if (col is CircleCollider2D circle)
                {
                    Gizmos.DrawWireSphere(circle.offset, circle.radius);
                }
                else if (col is CapsuleCollider2D capsule)
                {
                    Gizmos.DrawWireCube(capsule.offset, capsule.size);
                }
                else
                {
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                }

                // คืนค่า Matrix เพื่อวาดเส้นทิศทาง
                Gizmos.matrix = Matrix4x4.identity;
                Vector3 center = col.bounds.center;
                Vector3 upDir = transform.up;
                float radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(center, center + (upDir * (radius + _stoppingDistance)));
            }
        }
#endif
    }
}