using System;

namespace Genoverrei.Library.DesignPatternCore
{
    [CreateAssetMenu(fileName = "AudioChannel", menuName = "GenoverreiLibrary/DesignPattern/Observer/AudioObserverChannel")]
    public class AudioChannelSO : ScriptableObject
    {
        public Action<AudioClip> OnSfxChannel;
        public Action<AudioClip> OnBmgChannel;
        public Action<AudioClip> OnVoiceoverChannel;

        public void SendSfxSignal(AudioClip sound) => OnSfxChannel?.Invoke(sound);

        public void SendBmgSignal(AudioClip sound) => OnBmgChannel?.Invoke(sound);

        public void SendVoiceoverSignal(AudioClip sound) => OnVoiceoverChannel?.Invoke(sound);
    }
}
