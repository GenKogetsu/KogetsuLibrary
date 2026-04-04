using System;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public class BasicMoveAbility2D : BaseMoveAbility2D, IFixedUpdateState
    {
        public void OnFixedUpdate()
        {
            if (Context == null || Context.Stats == null) return;
            Context.Rigidbody.linearVelocity = new Vector3(CurrentInput.x, CurrentInput.y , 0) * Context.Stats.GetMoveSpeed();
        }

        public override void ExecuteJump()
        {
            if (Context != null && Context.Stats != null)
                Context.Rigidbody.AddForce(Vector2.up * Context.Stats.GetJumpForce(), ForceMode2D.Impulse);
        }
    }
}
