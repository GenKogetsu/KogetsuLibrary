using System;
using Genoverrei.Library.Core;

[Serializable]
public struct VNConversationNode
{
    public VNConversationMode ConversationMode;

    public VNDialogueNode DialogueNode;
    public VNChoiceNode ChoiceNode;
}