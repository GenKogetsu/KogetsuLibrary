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
            if (string.IsNullOrEmpty(emotionName)) return; //new: guard (ternary was inverted)
            var emotion = _characterData.GetEmotion(emotionName); //new: fix — was using wrong branch

            if (emotion == null) return;

            _characterSprite.sprite = emotion.Value.EmoteSprite;
            _characterSpriteAnimator.Play(emotion.Value.EmoteClip.name);
        }

        private void OnAnimationSignel(string animationName)
        {
            if (string.IsNullOrEmpty(animationName)) return; //new: guard (ternary was inverted)
            var animation = _characterData.GetAnimation(animationName); //new: fix — was using wrong branch

            if (animation == null) return;

            _characterBehaviorAniamtor.Play(animation.Value.BehaviorAnimationClip.name);
        }

        private void OnSoundEffectSignel(string soundEffectName) //new: was missing entirely
        {
            if (string.IsNullOrEmpty(soundEffectName)) return;
            var soundEffect = _characterData.GetSoundEffect(soundEffectName);
            if (soundEffect == null) return;
            // ส่ง clip ออกไปผ่าน audio observer ถ้ามีการเชื่อมต่อในอนาคต — ตอนนี้ stub ไว้
        }

        private void OnEnable()
        {
            _characterData.OnVNEmotionChannel     += OnEmotionSignel;
            _characterData.OnVNAnimationChannel   += OnAnimationSignel;
            _characterData.OnVNSoundEffectChannel += OnSoundEffectSignel; //new
        }

        private void OnDisable()
        {
            _characterData.OnVNEmotionChannel     -= OnEmotionSignel;
            _characterData.OnVNAnimationChannel   -= OnAnimationSignel;
            _characterData.OnVNSoundEffectChannel -= OnSoundEffectSignel; //new
        }
    }
}