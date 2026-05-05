using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNInteractState
    {
        [MinValue(1)]
        public int TargetAnswerNumber;

        public float ReturnValue;

        public bool ReturnToChoicePhase;

        public List<VNConversationNode> SubConversation = new();
    }
}
