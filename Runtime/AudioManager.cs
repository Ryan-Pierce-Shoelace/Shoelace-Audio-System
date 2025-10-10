using System;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using ShoelaceStudios.AudioSystem.SoundPlayers;
using ShoelaceStudios.Utilities.Singleton;
using UnityEngine;

namespace ShoelaceStudios.AudioSystem
{
    public class AudioManager : PersistentSingleton<AudioManager>
    {
        [Header("Initial Setup")] [SerializeField]
        private SoundConfig startingMusic;

        [SerializeField] private bool playMusicOnStart = true;

        private Bus masterBus;
        private Bus musicBus;
        private Bus sfxBus;
        private Bus ambientBus;

        [SerializeField] [Range(0, 1)] private float masterVolume =
            1f;

        [SerializeField] [Range(0, 1)] private float musicVolume =
            1f;

        [SerializeField] [Range(0, 1)] private float sfxVolume =
            1f;

        [SerializeField] [Range(0, 1)] private float ambientVolume =
            1f;

        [Header("Fade Settings")] [SerializeField]
        private float defaultFadeTime =
            2f;

        private Dictionary<string, ISoundPlayer> activeSounds;
        private HashSet<SoundEmitter> activeEmitters;
        private MusicSystem musicSystem;

        #region Setup

        public void InitializeSystem()
        {
            activeSounds = new Dictionary<string, ISoundPlayer>();
            activeEmitters = new HashSet<SoundEmitter>();
            musicSystem = new MusicSystem();

            masterBus = RuntimeManager.GetBus("bus:/");
            musicBus = RuntimeManager.GetBus("bus:/Music");
            sfxBus = RuntimeManager.GetBus("bus:/SFX");
            ambientBus = RuntimeManager.GetBus("bus:/Ambience");

            UpdateAllVolumes();
        }

        public void PlayStartingMusic()
        {
            if (playMusicOnStart && startingMusic != null)
            {
                PlayMusic(startingMusic);
            }

            UpdateAllVolumes();
        }

        #endregion

        #region Volume Controls

        public void SetMasterVolume(float value)
        {
            masterBus.setVolume(masterVolume);
        }

        public void SetMusicVolume(float value)
        {
            musicBus.setVolume(musicVolume);
        }

        public void SetSFXVolume(float value)
        {
            sfxBus.setVolume(sfxVolume);
        }

        public void SetAmbientVolume(float value)
        {
            ambientBus.setVolume(ambientVolume);
        }


        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("MasterVolume", masterVolume);
                PlayerPrefs.Save();
                UpdateAllVolumes();
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
                PlayerPrefs.Save();
                UpdateAllVolumes();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
                PlayerPrefs.Save();
                UpdateAllVolumes();
            }
        }

        public float AmbientVolume
        {
            get => ambientVolume;
            set
            {
                ambientVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
                PlayerPrefs.Save();
                UpdateAllVolumes();
            }
        }


        private void UpdateAllVolumes()
        {
            masterBus.setVolume(masterVolume);
            musicBus.setVolume(musicVolume);
            sfxBus.setVolume(sfxVolume);
            ambientBus.setVolume(ambientVolume);
        }

        private float ConvertToFMODVolume(float sliderValue)
        {
            if (sliderValue <= 0) return 0.0001f;

            return sliderValue * sliderValue;
        }

        #endregion

        #region Sound Playback

        public void PlayOneShot(SoundConfig config, Vector3 position = default)
        {
            RuntimeManager.PlayOneShot(config.EventRef, position);
        }

        public ISoundPlayer CreateSound(SoundConfig config, Transform parent = null)
        {
            ISoundPlayer player = config.Is3D ? new AttachedSoundPlayer(config, parent) : new SimpleSoundPlayer(config);

            string id = Guid.NewGuid().ToString();
            activeSounds[id] = player;
            return player;
        }

        public void PlayMusic(SoundConfig music, float fadeTime = 2f)
        {
            musicSystem.PlayMusic(music, fadeTime);
        }

        public void StopMusic(float fadeTime = 2f)
        {
            musicSystem.StopMusic(fadeTime);
        }

        #endregion

        #region Emitter Management

        public void RegisterEmitter(SoundEmitter emitter)
        {
            activeEmitters.Add(emitter);
        }

        public void UnregisterEmitter(SoundEmitter emitter)
        {
            if (emitter != null)
            {
                activeEmitters.Remove(emitter);
            }
        }

        #endregion


        #region Cleanup

        public void StopAllSounds()
        {
            foreach (ISoundPlayer sound in activeSounds.Values)
            {
                sound.Stop();
            }

            foreach (SoundEmitter emitter in activeEmitters)
            {
                emitter.Stop();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                foreach (ISoundPlayer sound in activeSounds.Values)
                {
                    sound.Dispose();
                }

                activeSounds.Clear();

                foreach (SoundEmitter emitter in activeEmitters.Where(emitter => emitter != null))
                {
                    Destroy(emitter.gameObject);
                }

                activeEmitters.Clear();

                musicSystem?.Dispose();
            }
        }

        #endregion
    }
}