using System;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNChoicePhaseData
    {

        public VNDialogueMode DialogueMode;

        public bool ChangeBackground;
        public Sprite BackgroundSprite;

        public bool OverrideBmgClip;
        public AudioClip BmgClip;

        public bool UseAmbientEvent;
        public VNAmbientEventType AmbientEventType;

        public bool UseVoiceoverClip;
        public AudioClip VoiceoverClip;

        public bool OverrideDialogueBoxAnimation;
        public AnimationClip DialogueBoxAnimation;

        [SerializeReference]
        public List<VNSpeakerData> Speakers;
    }
}