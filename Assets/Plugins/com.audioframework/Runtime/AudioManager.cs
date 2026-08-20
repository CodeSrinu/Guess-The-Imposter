using UnityEngine;
using UnityEngine.Audio;

namespace AudioFramework
{
    public class AudioManager: MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer _mainMixer;

        [Header("SFX")]
        [SerializeField] private int _sfxSourcesCount;

        private MixerController _mixerController;
        private BGMPlayer _bgmPlayer;
        private SFXPool _sfxPool;
        private VoicePlayer _voicePlayer;
        private AmbientPlayer _ambientPlayer;
        private UIPlayer _uiPlayer;

        [Header("Catalog")]
        public CatalogLoadLocation loadLocation = CatalogLoadLocation.Local;

        [SerializeField] private AudioCatalog _audioCatalog;
        [SerializeField] private AudioCatalogAddressables _audioCatalogAdressables;

        public enum CatalogLoadLocation
        {
            Local,
            Remote
        }

        private AudioSource _bgmSource1;
        private AudioSource _bgmSource2;
        private AudioSource _voiceSource;
        private AudioSource _uiSource;


        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Start()
        {
            LoadVolumes();
        }


        private void Initialize()
        {
            _mixerController = new MixerController(_mainMixer);


            _bgmSource1 = gameObject.AddComponent<AudioSource>();
            _bgmSource2 = gameObject.AddComponent<AudioSource>();
            _bgmPlayer = new BGMPlayer(
                _mixerController.GetGroup("BGM"),
                this
                );

            _bgmPlayer.SetSources(_bgmSource1, _bgmSource2);


            _sfxPool = new SFXPool(_sfxSourcesCount, this, _mixerController.GetGroup("SFX"));

            _voiceSource = gameObject.AddComponent<AudioSource>();
            _voicePlayer = new VoicePlayer(_voiceSource, _mixerController.GetGroup("Voice"));

            _ambientPlayer = new AmbientPlayer(_mixerController.GetGroup("Ambient"), this);

            _uiSource = gameObject.AddComponent<AudioSource>();
            _uiPlayer = new UIPlayer(_uiSource, _mixerController.GetGroup("UI"));


            if(loadLocation == CatalogLoadLocation.Local && _audioCatalog != null)
                _audioCatalog.Initialize();
            else if(_audioCatalogAdressables != null)
                _audioCatalogAdressables.Initialize();

        }

        public static async void PlayMusic(string key)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);

            if (clip != null)
                Instance._bgmPlayer.Play(clip);
        }

        public static void StopMusic()
        {
            Instance._bgmPlayer.Stop();
        }

        public static void PauseMusic()
        {
            Instance._bgmPlayer.Pause();
        }

        public static void ResumeMusic()
        {
            Instance._bgmPlayer.Resume();
        }

        public static async void CrossFadeMusic(string key, float duration)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);
            if(clip == null) return;

            Instance._bgmPlayer.CrossFade(clip, duration);
        }

        public static void SetMusicVolume(float vol)
        {
            Instance._mixerController.SetVolume("MusicVolume", vol);
            Instance.SaveVolumes();
        }
        public static void SetSFXVolume(float vol)
        {
            Instance._mixerController.SetVolume("SFXVolume", vol);
            Instance.SaveVolumes();
        }
        public static void SetVoiceVolume(float vol)
        {
            Instance._mixerController.SetVolume("VoiceVolume", vol);
            Instance.SaveVolumes();

        }
        public static void SetAmbientVolume(float vol)
        {
            Instance._mixerController.SetVolume("AmbientVolume", vol);
            Instance.SaveVolumes();
        }
        public static void SetUIVolume(float vol)
        {
            Instance._mixerController.SetVolume("UIVolume", vol);
            Instance.SaveVolumes();
        }


        public static float GetMusicVolume()
        {
            return Instance._mixerController.GetVolume("MusicVolume");
        }
        public static float GetSFXVolume()
        {
            return Instance._mixerController.GetVolume("SFXVolume");
        }
        public static float GetVoiceVolume()
        {
            return Instance._mixerController.GetVolume("VoiceVolume");
        }
        public static float GetAmbientVolume()
        {
            return Instance._mixerController.GetVolume("AmbientVolume");
        }
        public static float GetUIVolume()
        {
            return Instance._mixerController.GetVolume("UIVolume");
        }

        public static async void PlayUISound(string key)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);

            if (clip == null) return;

            Instance._uiPlayer.PlayUI(clip);
        }

        public static async void PlaySFX(string key)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);

            if (clip == null) return;

            Instance._sfxPool.PlaySFX(clip);
        }
        public static async void PlaySFXDelayed(string key, float delay)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);

            if (clip == null) return;

            Instance._sfxPool.PlayDelayedSFX(clip, delay);
        }

        public static async void PlayVoice(string key)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);

            if (clip == null) return;

            Instance._voicePlayer.PlayVoice(clip);
        }

        public static async void PlayAmbient(string ambientKey)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(ambientKey)
                : await Instance._audioCatalogAdressables.GetClipAsync(ambientKey);

            if (clip == null) return;

            Instance._ambientPlayer.PlayAmbient(clip, ambientKey);
        }

        public static void StopAllAmbient()
        {
            Instance._ambientPlayer.StopAllAmbient();
        }

        public static void StopAmbientByKey(string ambientKey)
        {
            Instance._ambientPlayer.StopAmbientByKey(ambientKey);
        }

        public static async void FadeInMusic(string key, float fadeInTime)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(key)
                : await Instance._audioCatalogAdressables.GetClipAsync(key);

            if (clip == null) return;

            Instance._bgmPlayer.FadeInBGM(clip, fadeInTime);
        }
        public static void FadeOutMusic(float fadeInTime)
        {
            Instance._bgmPlayer.FadeOutBGM(fadeInTime);
        }

        public static async void FadeInAmbient(string ambientKey, float fadeInTime)
        {
            AudioClip clip = Instance.loadLocation == CatalogLoadLocation.Local
                ? Instance._audioCatalog.GetClip(ambientKey)
                : await Instance._audioCatalogAdressables.GetClipAsync(ambientKey);

            if (clip == null) return;

            Instance._ambientPlayer.FadeInAmbient(clip, ambientKey, fadeInTime);
        }
        public static void FadeOutAmbient(string ambientKey, float fadeInTime)
        {
            Instance._ambientPlayer.FadeOutAmbient(ambientKey, fadeInTime);
        }




        private void SaveVolumes()
        {

            PlayerPrefs.SetFloat("MusicVolume", GetMusicVolume());
            PlayerPrefs.SetFloat("SFXVolume", GetSFXVolume());
            PlayerPrefs.SetFloat("VoiceVolume", GetVoiceVolume());
            PlayerPrefs.SetFloat("AmbientVolume", GetAmbientVolume());
            PlayerPrefs.SetFloat("UIVolume", GetUIVolume());
            PlayerPrefs.Save();
        }

        private void LoadVolumes()
        {
            Instance._mixerController.SetVolume("MusicVolume", PlayerPrefs.GetFloat("MusicVolume", 1f));
            Instance._mixerController.SetVolume("SFXVolume", PlayerPrefs.GetFloat("SFXVolume", 1f));
            Instance._mixerController.SetVolume("VoiceVolume", PlayerPrefs.GetFloat("VoiceVolume", 1f));
            Instance._mixerController.SetVolume("AmbientVolume", PlayerPrefs.GetFloat("AmbientVolume", 1f));
            Instance._mixerController.SetVolume("UIVolume", PlayerPrefs.GetFloat("UIVolume", 1f));
        }

        private void OnDestroy()
        {
            if(loadLocation == CatalogLoadLocation.Remote)
                _audioCatalogAdressables?.ReleaseAllAssets();
        }
    }
}
