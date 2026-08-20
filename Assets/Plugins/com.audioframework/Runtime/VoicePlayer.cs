using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class VoicePlayer
    {
        private AudioSource _audioSource;

        public VoicePlayer(AudioSource audioSource, AudioMixerGroup mixerGroup)
        {
            _audioSource = audioSource;
            _audioSource.outputAudioMixerGroup = mixerGroup;
        }

        public void PlayVoice(AudioClip clip)
        {
            if (_audioSource.isPlaying) 
                _audioSource.Stop();

            _audioSource.PlayOneShot(clip);
        }
    }
}
