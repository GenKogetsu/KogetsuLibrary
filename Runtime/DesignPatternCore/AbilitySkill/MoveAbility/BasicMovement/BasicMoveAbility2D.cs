using System;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public class BasicMoveAbility2D : BaseMoveAbility2D, IEnterState, IFixedUpdateState , IExitState
    {
        public void OnEnter()
        {
            if (InputObserverChannel != null) InputObserverChannel.OnMoveChannel += SetInput;
            if (InputObserverChannel != null) InputObserverChannel.OnJumpChannel += ExecuteJump;
        }

        public void OnExit()
        {
            if (InputObserverChannel != null) InputObserverChannel.OnMoveChannel -= SetInput;
            if (InputObserverChannel != null) InputObserverChannel.OnJumpChannel -= ExecuteJump;
        }

        public void OnFixedUpdate()
        {
            if (Context == null || Context.Stats == null) return;
            Context.Rigidbody.linearVelocity = new Vector3(CurrentInput.x, CurrentInput.y , 0) * Context.Stats.GetMoveSpeed();
        }

        public void ExecuteJump()
        {
            if (Context != null && Context.Stats != null)
                Context.Rigidbody.AddForce(Vector2.up * Context.Stats.GetJumpForce(), ForceMode2D.Impulse);
        }
    }
}
