using UnityEngine;
using Genoverrei.Library.DesignPatternCore;

namespace Genoverrei.Library.Core
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Obsever Channels")]
        [Required]
        [SerializeField] private AudioObserverSO _audioObserver;

        [Header("Audio Source")]

        [Required]
        [SerializeField] private AudioSource _sfx;

        [Required]
        [SerializeField] private AudioSource _bmg;

        [Required]
        [SerializeField] private AudioSource _voiceover;

        private void OnEnable()
        {
            _audioObserver.OnSfxChannel += OnSfxSignal;
            _audioObserver.OnBmgChannel += OnBmgSignal;
            _audioObserver.OnVoiceoverChannel += OnVoiceoverSignal;
        }

        public void OnSfxSignal(AudioClip sound)
        {
            if (sound == null)
            {
                _sfx.Stop();
                _sfx.clip = null;
                return;
            }

            _sfx.PlayOneShot(sound);
        }

        public void OnBmgSignal(AudioClip sound)
        {
            if (sound == null)
            {
                _bmg.Stop();
                _bmg.clip = null;
                return;
            }

            _bmg.clip = sound;
            _bmg.Play();
        }

        public void OnVoiceoverSignal(AudioClip sound)
        {
            if (sound == null)
            {
                _voiceover.Stop();
                _voiceover.clip = null;
                return;
            }

            _voiceover.PlayOneShot(sound);
        }
    }
}
