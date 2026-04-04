using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNChoiceNode
    {
        public VNQuestionState QuestionState;

        public VNAnswerState AnswerState;

        public List<VNInteractState> InteractStates;
    }
}