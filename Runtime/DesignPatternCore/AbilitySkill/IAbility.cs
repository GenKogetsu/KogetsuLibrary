namespace Genoverrei.Library
{
    public interface IAbility { }

    public interface IMoveAbility 
    {
        void SetInput(Vector3 input);
    }

    public interface IAbilityState<T> 
    {
        void Enter(T controller);
        void UpdateLogic();
        void FixedUpdateLogic();
        void Exit();
    }
}