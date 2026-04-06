using System;
using Genoverrei.Library.Core;

[Serializable]
public class VNConversationNode
{
    public VNConversationMode ConversationMode;

    public VNDialogueNode DialogueNode;
    public VNChoiceNode ChoiceNode;
}