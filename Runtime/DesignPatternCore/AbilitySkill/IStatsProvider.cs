namespace Genoverrei.Library.DesignPatternCore;

public interface IStatsProvider
{
    
}

public interface IMoveStatsProvider : IStatsProvider
{
    float GetMoveSpeed();
    float GetJumpForce();
}