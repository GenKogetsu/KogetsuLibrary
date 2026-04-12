using System;
using UnityEngine;

namespace Genoverrei.Library.Core
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MoveController2D : BaseMoveController<IMoveContext2D, BaseMoveAbility2D>, IMoveContext2D
    {
        [ReadOnly]
        [SerializeField] protected Rigidbody2D Rb2;

        [Tooltip("Perspective Mode — Auto-rotates on all axes for movement.")]
        [SerializeField] protected bool PerspectiveMode;

        [MaxValue(90f)]
        [SerializeField] protected Vector2 PerspectiveQuaternion2D;

        protected override IMoveContext2D GetContext() => this;

        Rigidbody2D IMoveContext2D.Rb2 => Rb2;

#if UNITY_EDITOR  

        protected virtual void OnDrawGizmosSelected()
        {
            if (MoveAbility is ClickMoveAbility2D clickAbility) clickAbility.DrawGizmos(transform);
        }

        protected override void OnValidate()
        {
            if (Application.isPlaying) return;

            if (!Rb2) TryGetComponent(out Rb2);

            if (PerspectiveQuaternion2D.x < 0) PerspectiveQuaternion2D.x *= -1;

            if (PerspectiveQuaternion2D.y < 0) PerspectiveQuaternion2D.y *= -1;
        }
#endif
        protected override void Awake()
        {
            base.Awake();
            if (!Rb2) TryGetComponent(out Rb2);
        }

        public override float VerticalVelocity => Rb2 != null ? Rb2.linearVelocity.y : 0f;

        /// <summary>
        /// <para>TH: หมุน Vector ของ Input ตามองศาของ PerspectiveQuaternion</para>
        /// <para>EN: Rotates the input vector by PerspectiveQuaternion using Euler angles.</para>
        /// </summary>
        protected override Vector3 TransformInput(Vector3 input)
        {
            if (!PerspectiveMode || input == Vector3.zero) return input;

            var rotation = new Vector3(PerspectiveQuaternion2D.x,0, PerspectiveQuaternion2D.y);

            Quaternion quaternion = Quaternion.Euler(rotation);
            return quaternion * input;
        }

        public Rigidbody2D GetRigidbody2D() => Rb2;

        public bool GetPerspectiveMode() => PerspectiveMode;

        public Vector2 GetPerspectiveQuaternion2D() => PerspectiveQuaternion2D;
    }
}