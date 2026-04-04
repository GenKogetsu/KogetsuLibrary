using System;
using UnityEngine;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public class BasicMoveAbility3D : BaseMoveAbility3D, IFixedUpdateState
    {
        [Header("Movement Settings")]
        [SerializeField] private bool _enableRotation = true;
        [SerializeField] private float _rotationSpeed = 15f;

        [Header("Ball Settings (Physics Rolling)")]
        [SerializeField] private bool _isBallRolling = false;
        [SerializeField] private float _rollTorque = 10f;

        public void OnFixedUpdate()
        {
            if (Context?.Stats == null || Context.Rigidbody == null) return;

            Transform camTransform = Camera.main.transform;
            Vector3 moveDirection = Vector3.zero;

            if (CurrentInput.sqrMagnitude > 0.001f)
            {
                Vector3 camForward = camTransform.forward;
                Vector3 camRight = camTransform.right;

                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                moveDirection = (camForward * CurrentInput.z + camRight * CurrentInput.x).normalized;
            }

            // 🚀 1. จัดการเรื่องความเร็ว (Velocity)
            Vector3 targetVelocity = moveDirection * Context.Stats.GetMoveSpeed();
            Context.Rigidbody.linearVelocity = new Vector3(targetVelocity.x, Context.Rigidbody.linearVelocity.y, targetVelocity.z);

            // 🚀 2. ถ้าเปิดโหมดลูกบอล (กลิ้งได้)
            if (_isBallRolling && moveDirection != Vector3.zero)
            {
                // คำนวณหาแกนที่จะหมุน (แกนที่ตั้งฉากกับทิศทางการเคลื่อนที่)
                Vector3 rollAxis = Vector3.Cross(Vector3.up, moveDirection);
                Context.Rigidbody.AddTorque(rollAxis * _rollTorque, ForceMode.Force);
            }
            // 🚀 3. ถ้าโหมดปกติ (หันหน้าตามทิศทาง)
            else if (_enableRotation && moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                Context.Rigidbody.MoveRotation(Quaternion.Slerp(Context.Rigidbody.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
            }
        }

        public override void ExecuteJump()
        {
            if (Context?.Stats != null)
                Context.Rigidbody.AddForce(Vector3.up * Context.Stats.GetJumpForce(), ForceMode.Impulse);
        }
    }
}