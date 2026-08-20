using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class MixerController
    {
        private AudioMixer _mixer;

        public MixerController(AudioMixer mixer)
        {
            _mixer = mixer;
        }

        public void SetVolume(string parameterName, float volume)
        {
            float db = volume > 0.0000001f ? Mathf.Log10(volume) * 20f : -80f;
            _mixer.SetFloat(parameterName, db);
        }

        public float GetVolume(string parameterName)
        {
            _mixer.GetFloat(parameterName, out float db);
            return Mathf.Pow(10f, db/20f);
        }

        public AudioMixerGroup GetGroup(string groupName)
        {
            AudioMixerGroup[] groups = _mixer.FindMatchingGroups(groupName);

            return groups.Length > 0 ? groups[0] : null;
        }
    }
}
