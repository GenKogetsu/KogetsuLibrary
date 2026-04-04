using System;
using NaughtyAttributes;
using Genoverrei.Library.Core;

namespace Genoverrei.Library.DesignPatternCore
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตัวควบคุมการแสดงผลและแอนิเมชันของตัวละคร </para>
    /// <para> (EN) : Controller for character display and animation. </para>
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public sealed class VNCharacterController : MonoBehaviour
    {
        [Header("AssignReferences")]

        [Required]
        [SerializeField] private Animator _characterBehaviorAniamtor;

        [Required]
        [SerializeField] private Animator _characterSpriteAnimator;

        [Required] 
        [SerializeField] private SpriteRenderer _characterSprite;

        [Header("AssignData")]
        [Required]
        [SerializeField] private VNCharacterSO _characterData;

        private void OnEmotionSignel(string emotionName)
        {
            var emotion = string.IsNullOrEmpty(emotionName) ? _characterData.GetEmotion(emotionName): null;

            if (emotion == null) return;

            var spriteTarget = emotion.Value.EmoteSprite;

            var clipName = emotion.Value.EmoteClip.name;

            _characterSprite.sprite = spriteTarget;

            _characterSpriteAnimator.Play(clipName);
        }

        private void OnAnimationSignel(string animationName)
        {
            var animation = string.IsNullOrEmpty(animationName) ? _characterData.GetAnimation(animationName) : null;

            if (animation == null) return;

            _characterBehaviorAniamtor.Play(animationName);
        }

        private void OnEnable()
        {
            _characterData.OnVNEmotionChannel += OnEmotionSignel;
            _characterData.OnVNAnimationChannel += OnAnimationSignel;
        }

        private void OnDisable()
        {
            _characterData.OnVNEmotionChannel -= OnEmotionSignel;
            _characterData.OnVNAnimationChannel -= OnAnimationSignel;
        }
    }
}