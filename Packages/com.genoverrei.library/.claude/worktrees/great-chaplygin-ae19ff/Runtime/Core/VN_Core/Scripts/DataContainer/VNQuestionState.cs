using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNQuestionState
    {
        public bool UseEnterPhase;
        public VNDialoguePhaseData EnterPhase;

        public VNDialoguePhaseData MainPhase;
    }
}