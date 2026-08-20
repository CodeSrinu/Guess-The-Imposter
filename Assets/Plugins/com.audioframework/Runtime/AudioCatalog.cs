using System.Collections.Generic;
using UnityEngine;

namespace AudioFramework
{
    [CreateAssetMenu(fileName = "AudioCatalog", menuName = "AudioFrameWork/AudioCatalog")]
    public class AudioCatalog:ScriptableObject
    {
        [System.Serializable]
        public struct AudioEntry
        {
            public string key;
            public AudioClip clip;
        }
        [SerializeField]
        private List<AudioEntry> _entries = new List<AudioEntry>();
        private Dictionary<string, AudioClip> _entriesDictionary;

        public void Initialize()
        {
            _entriesDictionary = new Dictionary<string, AudioClip>();

            foreach (var entry in _entries)
            {
                if(entry.clip != null && !string.IsNullOrEmpty(entry.key))
                {
                    _entriesDictionary[entry.key] = entry.clip;
                }
            }
        }

        public AudioClip GetClip(string key)
        {
            if(_entriesDictionary.TryGetValue(key, out AudioClip clip))
            {
                return clip; 
            }
            Debug.LogWarning($"AudioCatalog: {key} key is not found");
            return null;
        }

        
    }
}
