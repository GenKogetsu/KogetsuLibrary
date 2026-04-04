namespace Genoverrei.Library.DesignPatternCore
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveController3D : BaseMoveController<IMoveContext3D, BaseMoveAbility3D>, IMoveContext3D
    {
        public Rigidbody Rigidbody { get; private set; }

        protected override IMoveContext3D GetContext() => this;

        protected override void Awake()
        {
            base.Awake();
            if (this.TryGetComponent<Rigidbody>(out var rigidbody)) Rigidbody = rigidbody;
        }
    }
}
