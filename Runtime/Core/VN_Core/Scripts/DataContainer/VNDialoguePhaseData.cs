
using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNDialoguePhaseData : VNChoicePhaseData
    {
        [TextArea(3, 5)]
        public string DialogueText;

        [SerializeReference]
        public VNTextSettings TextSettings;
    }
}
