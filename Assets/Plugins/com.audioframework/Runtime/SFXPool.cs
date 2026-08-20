using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class SFXPool
    {
        private MonoBehaviour _coroutineRunner;
        private AudioMixerGroup _mixerGroup;
        private List<AudioSource> _pool;



        public SFXPool(int sfxSourcesCount, MonoBehaviour coroutineRunner, AudioMixerGroup mixerGroup)
        {
            _coroutineRunner = coroutineRunner;
            _mixerGroup = mixerGroup;
            _pool = new List<AudioSource>();
            CreateAudioSources(sfxSourcesCount);
        }

        private void CreateAudioSources(int count)
        {
            for(int i = 0; i < count; i++)
            {
                AudioSource source = _coroutineRunner.gameObject.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = _mixerGroup;
                source.playOnAwake = false;
                _pool.Add(source);
            }
        }


        private AudioSource GetAudioSource()
        {
            foreach(AudioSource source in _pool)
            {
                if(!source.isPlaying)
                    return source;
            }

            AudioSource newSource = _coroutineRunner.gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = _mixerGroup;
            newSource.playOnAwake = false;
            _pool.Add(newSource);
            return newSource;
        }

        public void PlaySFX(AudioClip clip)
        {
            AudioSource source = GetAudioSource();
            source.playOnAwake = false;
            source.PlayOneShot(clip);
        } 

        public void PlayDelayedSFX(AudioClip clip, float delay)
        {
            _coroutineRunner.StartCoroutine(DelayedPlay(clip, delay));
        }

        private IEnumerator DelayedPlay(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlaySFX(clip);
        }
    }
}
