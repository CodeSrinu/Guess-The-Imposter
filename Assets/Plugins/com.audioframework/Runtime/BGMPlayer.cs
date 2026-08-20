using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class BGMPlayer
    {
        private AudioMixerGroup _mixerGroup;
        private AudioSource _currentSource;
        private AudioSource _prevSource;
        private MonoBehaviour _coroutineRunner;

        private Coroutine crossFadecoroutine;
        private Coroutine fadeIncoroutine;
        private Coroutine fadeOutcoroutine;

        public BGMPlayer(AudioMixerGroup mixerGroup, MonoBehaviour coroutineRunner)
        {
            _mixerGroup = mixerGroup;
            _coroutineRunner = coroutineRunner; 
        }

        public void SetSources(AudioSource currentSource, AudioSource prevSource)
        {
            _currentSource = currentSource;
            _prevSource = prevSource;
            _currentSource.outputAudioMixerGroup = _mixerGroup;
            _prevSource.outputAudioMixerGroup = _mixerGroup;
            _currentSource.loop = true;
            _prevSource.loop = true;
        }

        public void Play(AudioClip newClip)
        {
            if (newClip == _currentSource.clip) return;

            _currentSource.clip = newClip;
            _currentSource.Play();
        }

        public void Stop()
        {
            _currentSource.clip = null;
            _currentSource.Stop();
        }

        public void Pause()
        {
            if(_currentSource.isPlaying)
                _currentSource.Pause();
        }

        public void Resume()
        {
            _currentSource.UnPause();
        }

        public void CrossFade(AudioClip clip, float fadeTime)
        {
            if(crossFadecoroutine != null)
                _coroutineRunner.StopCoroutine(crossFadecoroutine);

            _coroutineRunner.StartCoroutine(CrossFadeRoutine(clip, fadeTime));
        }

        private IEnumerator CrossFadeRoutine(AudioClip newClip, float duration)
        {
            AudioSource temp = _currentSource;
            _currentSource = _prevSource;
            _prevSource = temp;

            _currentSource.clip = newClip;
            _currentSource.volume = 0f;
            _currentSource.Play();

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / duration);
                _currentSource.volume = Mathf.Lerp(0f, 1f, t);
                _prevSource.volume = Mathf.Lerp(1f, 0f, t);

                yield return null;
            }

            _prevSource.Stop();

            _prevSource.volume = 1f;
            _currentSource.volume = 1f;
        }


        public void FadeOutBGM(float fadeOutTime)
        {
            if(fadeOutcoroutine != null)
                _coroutineRunner.StopCoroutine(fadeOutcoroutine);

             fadeOutcoroutine = _coroutineRunner.StartCoroutine(FadeOutRoutine(fadeOutTime));
        }
        
        private IEnumerator FadeOutRoutine(float fadeOutTime)
        {
            float startVolume = _currentSource.volume;

            float timer = 0f;

            while (timer < fadeOutTime)
            {
                timer += Time.deltaTime;
                float t = Mathf .Clamp01(timer / fadeOutTime);
                _currentSource.volume = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            _currentSource.Stop();
            _currentSource.volume = startVolume;
        }

        public void FadeInBGM(AudioClip clip, float fadeInTime)
        {
            if(fadeIncoroutine != null)
                _coroutineRunner.StopCoroutine(fadeIncoroutine);

           fadeIncoroutine = _coroutineRunner.StartCoroutine(FadeInRoutine(clip, fadeInTime));
        }

        private IEnumerator FadeInRoutine(AudioClip clip, float fadeInTime)
        {
            _currentSource.volume = 0f;
            _currentSource.clip = clip;
            _currentSource.Play();

            float timer = 0f;

            while(timer < fadeInTime)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeInTime);
                _currentSource.volume = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            _currentSource.volume = 1f;
        }
    }
}
