using Genoverrei.Library.DesignPatternCore;

public interface IMoveContext
{
    Transform Transform { get; }
    IMoveStatsProvider Stats { get; }
}

public interface IMoveContext2D : IMoveContext
{
    Rigidbody2D Rigidbody { get; }
}

public interface IMoveContext3D : IMoveContext
{
    Rigidbody Rigidbody { get; }
}