# Audio Framework
**Version 1.0.0** | Unity 6 | By Srinu, Badri and Bobby| Allreal Labs

A reusable, drop-in audio management framework for Unity projects. Provides a centralized solution for all audio needs — background music, sound effects, voice, ambient, and UI sounds — with a single consistent API across any project.

---

## Installation

1. Open **Package Manager** (Window → Package Manager)
2. Click **+** → **Add package from disk**
3. Navigate to `com.audioframework/` and select `package.json`
4. The package appears as **Audio Framework** under your project packages

---

## Setup

### 1. Add AudioManager to your scene
Drag the `AudioManager` prefab from `Packages/Audio Framework/Assets/Prefabs/` into your scene. It persists across scene loads automatically.

### 2. Assign the Main Mixer
In the AudioManager Inspector, drag `MainMixer` from `Packages/Audio Framework/Assets/Mixer/` into the **Main Mixer** slot.

### 3. Create an Audio Catalog
Right click in Project → **Create → AudioFramework → AudioCatalog**

Add your audio clips with string keys:

| Key | Clip |
|---|---|
| MainTheme | your BGM clip |
| ButtonClick | your SFX clip |
| Welcome | your voice clip |
| ForestAmbient | your ambient clip |

Drag the catalog into the AudioManager's **Audio Catalog** slot.

### 4. Set SFX Pool Size
Set **SFX Sources Count** in the Inspector (recommended: 5–10).

---

## Usage

```csharp
// Background Music
AudioManager.PlayMusic("MainTheme");
AudioManager.FadeInMusic("MainTheme", 2f);
AudioManager.FadeOutMusic(2f);
AudioManager.CrossFadeMusic("BattleTheme", 3f);
AudioManager.PauseMusic();
AudioManager.ResumeMusic();
AudioManager.StopMusic();

// Sound Effects
AudioManager.PlaySFX("Explosion");
AudioManager.PlaySFXDelayed("Explosion", 2f);

// Voice
AudioManager.PlayVoice("Welcome");

// Ambient
AudioManager.PlayAmbient("ForestAmbient");
AudioManager.FadeInAmbient("Forest", 2f);
AudioManager.FadeOutAmbient("Forest", 2f);
AudioManager.StopAllAmbient();
AudioManager.StopAmbientByKey("Forest");

// UI Sounds
AudioManager.PlayUISound("ButtonClick");

// Volume Control (0.0 - 1.0)
AudioManager.SetMusicVolume(0.7f);
AudioManager.SetSFXVolume(0.8f);
AudioManager.SetVoiceVolume(1.0f);
AudioManager.SetAmbientVolume(0.5f);
AudioManager.SetUIVolume(0.9f);

// Get Current Volume
float vol = AudioManager.GetMusicVolume();
```

---

## Audio Categories

| Category | Description | Mixer Group |
|---|---|---|
| BGM | Background music with crossfade, fade in/out | BGM |
| SFX | Pooled one-shot sound effects with delayed playback | SFX |
| Voice | Single channel voice — interrupts on new clip | Voice |
| Ambient | Layered looping ambient sounds, individually controllable | Ambient |
| UI | Interface feedback sounds | UI |

---

## Addressables Support (Remote Loading)

The framework supports loading audio clips remotely via Unity Addressables.

### Setup
1. Install **Addressables** package via Package Manager
2. Right click → **Create → AudioFramework → AddressablesAudioCatalog**
3. Add entries with string keys and `AssetReferenceT<AudioClip>` references
4. Mark your clips as Addressable in the Inspector
5. Set your remote load path in Addressables Profiles
6. Build → **New Build → Default Build Script**
7. Upload `ServerData/` to your CDN or GitHub Pages

### Switch to Remote Mode
In the AudioManager Inspector, set **Load Location** to **Remote** and assign the Addressables catalog.

### Notes
- Local mode works in Editor Play mode
- Remote mode requires a built executable and deployed server data
- Volumes are loaded in `Start()` — Audio Mixer ignores `SetFloat` calls during `Awake()`

---

## Architecture

```
AudioManager (MonoBehaviour, Singleton)
├── AudioCatalog (ScriptableObject) — string key → AudioClip
├── AudioCatalogAddressables (ScriptableObject) — string key → remote AudioClip
├── MixerController — Unity Audio Mixer integration, dB conversion
├── BGMPlayer — music playback, crossfade, fade in/out
├── SFXPool — AudioSource pool for one-shot SFX
├── VoicePlayer — single channel voice playback
├── AmbientPlayer — layered looping ambient audio
└── UIPlayer — UI sound playback
```

---

## Volume Persistence

Volume settings are automatically saved to `PlayerPrefs` whenever changed and restored on next launch. No setup required.

---

## Demo Scene

Open `Assets/Scenes/Sample` to see all framework features in action. Use the UI buttons to trigger each audio category. Volume sliders update in real time and persist across sessions.

---

## Requirements

- Unity 6000.0 or later
- Unity Audio (built-in)
- Addressables 2.9.1+ (optional, for remote loading only)
