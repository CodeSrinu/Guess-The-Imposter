using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class AmbientPlayer
    {
        private MonoBehaviour _coroutineRunner;
        private AudioMixerGroup _mixerGroup;
        private Dictionary<string, AudioSource> _audioSources;

        private Coroutine fadeInCoroutine;
        private Coroutine fadeOutCoroutine;

        public AmbientPlayer(AudioMixerGroup mixerGroup, MonoBehaviour coroutineRunner)
        {
            _mixerGroup = mixerGroup;
            _coroutineRunner = coroutineRunner;
            _audioSources = new Dictionary<string, AudioSource>();
        }

        private void CreateNewSource(string key)
        {
            AudioSource newSource = _coroutineRunner.gameObject.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = _mixerGroup;
            _audioSources[key] = newSource;
        }

        public void PlayAmbient(AudioClip clip, string ambientKey)
        {
            if(!_audioSources.ContainsKey(ambientKey)) 
                CreateNewSource(ambientKey);

            AudioSource source = _audioSources[ambientKey];
            source.clip = clip;
            source.loop = true;
            source.Play();
        }

        public void StopAllAmbient()
        {
            foreach(AudioSource source in _audioSources.Values)
            {
                source.Stop();
            }
        }

        public void StopAmbientByKey(string key)
        {
            _audioSources.TryGetValue(key, out AudioSource source);
            source.Stop();
            _audioSources.Remove(key);
        }

        public void FadeOutAmbient( string key,float fadeOutTime)
        {
            if (fadeOutCoroutine != null)
                _coroutineRunner.StopCoroutine(fadeOutCoroutine);

            fadeOutCoroutine = _coroutineRunner.StartCoroutine(FadeOutRoutine(fadeOutTime, key));
        }

        private IEnumerator FadeOutRoutine(float fadeOutTime, string key)
        {
            float startVolume = _audioSources[key].volume;

            float timer = 0f;

            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeOutTime);
                _audioSources[key].volume = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            _audioSources[key].Stop();
            _audioSources[key].volume = startVolume;
        }

        public void FadeInAmbient(AudioClip clip,string key, float fadeInTime)
        {
            if (fadeInCoroutine != null)
                _coroutineRunner.StopCoroutine(fadeInCoroutine);

            fadeInCoroutine = _coroutineRunner.StartCoroutine(FadeInRoutine(clip, key, fadeInTime));
        }

        private IEnumerator FadeInRoutine(AudioClip clip, string key, float fadeInTime)
        {
            if (!_audioSources.ContainsKey(key))
                CreateNewSource(key);

            _audioSources[key].volume = 0f;
            _audioSources[key].clip = clip;
            _audioSources[key].Play();

            float timer = 0f;

            while (timer < fadeInTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeInTime);
                _audioSources[key].volume = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            _audioSources[key].volume = 1f;
        }
    }
}
