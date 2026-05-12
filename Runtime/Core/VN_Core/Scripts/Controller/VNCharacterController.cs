using NaughtyAttributes;
using Genoverrei.Library.Core;
using UnityEngine.UI;

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
        [SerializeField] private Image _characterSprite;

        [SerializeField] private Color32 _speakerColor  = Color.white;
        [SerializeField] private Color32 _listenerColor = new(160, 160, 160, 255);

        [Header("AssignData")]
        [Required]
        [SerializeField] private VNCharacterSO _characterData;

        private void Start()
        {
            _characterSprite.color = _listenerColor;
        }

        private void OnEmotionSignel(string emotionName)
        {
            if (string.IsNullOrEmpty(emotionName)) return;

            var emotion = _characterData.GetEmotion(emotionName);
            if (emotion == null) return;

            _characterSprite.color  = _speakerColor;
            _characterSprite.sprite = emotion.Value.EmoteSprite;
            _characterSpriteAnimator.Play(emotion.Value.EmoteClip.name);
        }

        private void OnAnimationSignel(string animationName)
        {
            if (string.IsNullOrEmpty(animationName)) return;

            var animation = _characterData.GetAnimation(animationName);
            if (animation == null) return;

            _characterSprite.color = _speakerColor;
            _characterBehaviorAniamtor.Play(animation.Value.BehaviorAnimationClip.name);
        }

        private void OnSoundEffectSignel(string soundEffectName)
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
            _characterData.OnVNSoundEffectChannel += OnSoundEffectSignel;
        }

        private void OnDisable()
        {
            _characterData.OnVNEmotionChannel     -= OnEmotionSignel;
            _characterData.OnVNAnimationChannel   -= OnAnimationSignel;
            _characterData.OnVNSoundEffectChannel -= OnSoundEffectSignel;
        }
    }
}
