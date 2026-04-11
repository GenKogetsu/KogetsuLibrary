namespace Genoverrei.Library.Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveController3D : BaseMoveController<IMoveContext3D, BaseMoveAbility3D>, IMoveContext3D
    {
        [SerializeField] protected Rigidbody Rb;

        protected override IMoveContext3D GetContext() => this;

        Rigidbody IMoveContext3D.Rb => Rb;

        public override float VerticalVelocity => Rb != null ? Rb.linearVelocity.y : 0f;

        protected override void Awake()
        {
            if (Rb == null) TryGetComponent(out Rb);
            base.Awake();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!Application.isPlaying) Awake();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (MoveAbility is ClickMoveAbility3D clickAbility)
                clickAbility.DrawGizmos(transform);
        }
#endif
    }
}
