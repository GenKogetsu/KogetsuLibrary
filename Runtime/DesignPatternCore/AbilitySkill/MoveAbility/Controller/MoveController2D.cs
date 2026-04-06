using UnityEngine;

namespace Genoverrei.Library.DesignPatternCore
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MoveController2D : BaseMoveController<IMoveContext2D, BaseMoveAbility2D>, IMoveContext2D
    {
        public Rigidbody2D Rigidbody { get; private set; }

        protected override IMoveContext2D GetContext() => this;

        protected override void Awake()
        {
            base.Awake();
            if (this.TryGetComponent<Rigidbody2D>(out var rigidbody)) Rigidbody = rigidbody;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (StartingAbility is ClickMoveAbility2D clickAbility)
            {
                clickAbility.DrawGizmos(transform);
            }
        }
#endif
    }
}