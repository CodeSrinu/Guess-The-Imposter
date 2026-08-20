using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AudioFramework
{
    [CreateAssetMenu(fileName = "AddressableAudioCatalog", menuName = "AudioFrameWork/AdressablesAudioCatalog")]
    public class AudioCatalogAddressables : ScriptableObject
    {

        [System.Serializable]
        public struct AudioEntry
        {
            public string assetKey;
            public AssetReferenceT<AudioClip> asset;
        }

        [SerializeField] private List<AudioEntry> _entries = new List<AudioEntry>();
        private Dictionary<string, AssetReferenceT<AudioClip>> _entriesDictionary;
        private Dictionary<string, AudioClip> _loadedClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AsyncOperationHandle<AudioClip>> _loadedHandles = new Dictionary<string, AsyncOperationHandle<AudioClip>>();

        public void Initialize()
        {
            _entriesDictionary = new Dictionary<string, AssetReferenceT<AudioClip>>();
            foreach (var entry in _entries)
            {
                if(entry.asset != null && !string.IsNullOrEmpty(entry.assetKey))
                    _entriesDictionary[entry.assetKey] = entry.asset;
            }
        }


        public async Task<AudioClip> GetClipAsync(string assetKey)
        {
            if (_loadedClips.TryGetValue(assetKey, out AudioClip audioClip))
            {
                return audioClip;
            }
            else
            {
                if (_entriesDictionary.TryGetValue(assetKey, out AssetReferenceT<AudioClip> assetRef))
                {
                    try
                    {
                        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(assetRef);
                        _loadedHandles[assetKey] = handle;

                        AudioClip clip = await handle.Task;
                        _loadedClips[assetKey] = clip;

                        return clip;
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"AudioCatalogAddressables: {assetKey} assest load failed due to {e.Message}");
                        return null;
                    }
                }
                Debug.LogWarning($"AudioCatalogAddressables: {assetKey} key is not found");
                return null;
            }
        } 

        public void ReleaseAllAssets()
        {
            foreach(var handle in _loadedHandles.Values)
            {
                Addressables.Release(handle);
            }
            _loadedHandles.Clear();
            _loadedClips.Clear();
        }
    }
}
