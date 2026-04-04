using Genoverrei.Library.Core;
using Genoverrei.Library.DesignPatternCore;


public record struct VNTypingSkipEvent(ushort CurrentConversationIndex , VNCurrentPhase CurrentPhase) : IEvent;
