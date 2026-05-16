using Genoverrei.Library.Core;

namespace Genoverrei.Library.DesignPatternCore;

public record struct GameStateEvent(GameState State) : IEvent;
