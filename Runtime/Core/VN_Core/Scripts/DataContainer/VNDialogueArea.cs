using System;
using TMPro;
using UnityEngine.UI;

namespace Genoverrei.Library.Core
{
    [Serializable]
    public class VNDialogueArea
    {
        public VNDialogueMode DialogueMode;

        [Required]
        public RectTransform DialogueBox;

        [Required]
        public Animator DialogueAnimator;

        public TextMeshProUGUI SpeakerNameTMP;
        public Image SpeakerNameIcon; //new: shown when VNNameDisplayMode.Icon

        public TextMeshProUGUI DialogueTMP;
    }
}
