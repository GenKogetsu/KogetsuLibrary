using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNChoicePhaseData
    {
        public VNDialogueMode DialogueMode;
        

        public bool OverrideBmgClip;
        public AudioClip BmgClip;

        public bool UseAmbientEvent;
        public VNAmbientEventType AmbientEventType;

        public bool UseVoiceoverClip;
        public AudioClip VoiceoverClip;

        public bool OverrideDialogueAnimation;
        public AnimationClip DialogueBoxAnimation;

        public List<VNSpeakerData> Speakers;
    }
}