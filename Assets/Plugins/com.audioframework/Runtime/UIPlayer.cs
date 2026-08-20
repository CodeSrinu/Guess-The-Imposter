using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class UIPlayer 
    {
        private AudioSource _audioSource;


        public UIPlayer(AudioSource audioSource, AudioMixerGroup mixerGroup)
        {
            _audioSource = audioSource;
            _audioSource.outputAudioMixerGroup = mixerGroup;
            _audioSource.playOnAwake = false;
        }

        public void PlayUI(AudioClip clip)
        {
            if(_audioSource.isPlaying)
                _audioSource.Stop();

            _audioSource.PlayOneShot(clip);
        }
    }
}
