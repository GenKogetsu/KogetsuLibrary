using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNInteractState : VNQuestionState
    {
        public bool UseExitPhase;
        public VNChoicePhaseData ExitPhase;

        [Min(1)]
        public int TargetAnswerNumber;
    }
}