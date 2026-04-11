using System;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class BasicMoveAbility2D : BaseMoveAbility2D
    {
        protected override void OnEnter()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel += SetInput;
            Context.InputObserverChannel.OnJumpChannel += ExecuteJump;
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel -= SetInput;
            Context.InputObserverChannel.OnJumpChannel -= ExecuteJump;
        }

        protected override void OnFixedUpdate()
        {
            if (Context?.Stats == null) return;

            Vector2 clamped = Vector2.ClampMagnitude(CurrentInput, 1f);
            Vector3 rawDir = Context.TransformInput(new(clamped.x, clamped.y));

            Context.CurrentDirection = Context.SnapDirection(rawDir);
            Context.Rb2.linearVelocity = rawDir * Context.Stats.GetMoveSpeed();
        }

        protected void ExecuteJump()
        {
            if (Context?.Stats == null) return;
            Context.Rb2.AddForce(Vector2.up * Context.Stats.GetJumpForce(), ForceMode2D.Impulse);
        }
    }
}
