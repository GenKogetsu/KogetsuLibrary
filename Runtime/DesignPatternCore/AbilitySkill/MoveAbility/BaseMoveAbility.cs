using System;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public abstract class BaseMoveAbility<TContext> : BaseState<TContext> where TContext : class, IMoveContext
    {
        [SerializeField] protected BasicObserverChannelSO InputObserverChannel;

        protected Vector3 CurrentInput;
        protected virtual void SetInput(Vector3 input) => CurrentInput = input;
    }

    [Serializable] public abstract class BaseMoveAbility2D : BaseMoveAbility<IMoveContext2D> { }
    [Serializable] public abstract class BaseMoveAbility3D : BaseMoveAbility<IMoveContext3D> { }
}
