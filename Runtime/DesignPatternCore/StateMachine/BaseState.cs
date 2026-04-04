using System;

namespace Genoverrei.Library.DesignPatternCore
{
    [Serializable]
    public abstract class BaseState<TContext> : IState
    {
        protected TContext Context;
        public virtual void Initialize(TContext context) => Context = context;
    }
}
