using System;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public class ClickMoveAbility3D : BaseMoveAbility3D, IEnterState, IFixedUpdateState, IExitState
    {
        [Header("Click Move Settings")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _solidLayer;

        [Tooltip("ระยะเผื่อก่อนชนเป้าหมายหรือกำแพง")]
        [Range(0.5f, 0.01f)]
        [SerializeField] private float _stoppingDistance = 0.1f;

        [Range(15f, 100f)]
        [SerializeField] private float _rotationSpeed = 15f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugRay = true;
        [Tooltip("ปรับสเกลของเส้น Debug ได้อิสระ (ปกติ = 1)")]
        [SerializeField] private float _debugShapeScale = 1f;

        private Vector3 _targetPosition;
        private bool _hasTarget;
        private Camera _mainCamera;
        private RaycastHit _hit;
        private Collider _collider;

        public void OnEnter()
        {
            _mainCamera = Camera.main;
            if (Context?.Transform != null && _collider == null) _collider = Context.Transform.GetComponent<Collider>();

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

            Ray ray = _mainCamera.ScreenPointToRay(clickData.ClickPosition);
            if (Physics.Raycast(ray, out _hit, 1000f, _groundLayer))
            {
                _targetPosition = _hit.point;
                _hasTarget = true;
            }
        }

        public void OnFixedUpdate()
        {
            if (!_hasTarget || Context?.Stats == null || Context.Rigidbody == null || _collider == null) return;

            Vector3 centerPos = _collider.bounds.center;
            float radius = Mathf.Max(_collider.bounds.extents.x, _collider.bounds.extents.z);
            float totalStoppingDistance = radius + _stoppingDistance;

            Vector3 targetPosFlat = new(_targetPosition.x, centerPos.y, _targetPosition.z);
            float distance = Vector3.Distance(centerPos, targetPosFlat);

            Vector3 directionToTarget = (targetPosFlat - centerPos).normalized;

            // เช็คว่าชนกำแพงด้านหน้าด้วยความกว้างเท่าตัวละครหรือไม่
            bool hitWall = Physics.SphereCast(centerPos, radius, directionToTarget, out _hit, _stoppingDistance, _solidLayer);

            // ถ้าถึงเป้าหมาย หรือ "ชนกำแพง" ให้หยุดทันที ไม่มีการไถล
            if (distance <= totalStoppingDistance || hitWall)
            {
                _hasTarget = false;
                Context.Rigidbody.linearVelocity = new(0, Context.Rigidbody.linearVelocity.y, 0);
                return;
            }

            // เคลื่อนที่ตรงไปหาเป้าหมาย
            Vector3 targetVelocity = directionToTarget * Context.Stats.GetMoveSpeed();
            Context.Rigidbody.linearVelocity = new(targetVelocity.x, Context.Rigidbody.linearVelocity.y, targetVelocity.z);

            // หมุนตัว
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                Context.Rigidbody.MoveRotation(Quaternion.Slerp(Context.Rigidbody.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
            }
        }

#if UNITY_EDITOR
        public void DrawGizmos(Transform transform)
        {
            if (Application.isPlaying || !_showDebugRay) return;

            if (transform.TryGetComponent(out Collider col))
            {
                Gizmos.color = Color.cyan;

                // แมปรูปทรงและสเกลของ Transform มาใช้กับ Gizmos ทำให้ถ้ายืดวงรี เส้นก็จะวงรีตาม
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale * _debugShapeScale);

                // วาดรูปร่างเป๊ะๆ จาก Setting ของ Collider
                if (col is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
                else if (col is CapsuleCollider capsule)
                {
                    Gizmos.DrawWireCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
                }
                else if (col is MeshCollider meshCol && meshCol.sharedMesh != null)
                {
                    Gizmos.DrawWireMesh(meshCol.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one);
                }
                else
                {
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                }

                // คืนค่า Matrix เพื่อวาดเส้นทิศทางตรงๆ ไม่ให้เบี้ยว
                Gizmos.matrix = Matrix4x4.identity;
                Vector3 center = col.bounds.center;
                Vector3 forward = transform.forward;
                float radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(center, center + (forward * (radius + _stoppingDistance)));
            }
        }
#endif
    }
}