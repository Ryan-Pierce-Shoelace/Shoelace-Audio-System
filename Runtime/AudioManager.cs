using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using ShoelaceStudios.Utilities.Singleton;
using UnityEngine;

namespace ShoelaceStudios.AudioSystem
{
	public class AudioManager : PersistentSingleton<AudioManager>
	{
		[Header("Initial Setup")]
		[SerializeField] private SoundConfig startingMusic;
		[SerializeField] private bool playMusicOnStart = true;

		private Bus masterBus;
		private Bus musicBus;
		private Bus sfxBus;
		private Bus ambientBus;

		[SerializeField] [Range(0, 1)] private float masterVolume = 1f;
		[SerializeField] [Range(0, 1)] private float musicVolume = 1f;
		[SerializeField] [Range(0, 1)] private float sfxVolume = 1f;
		[SerializeField] [Range(0, 1)] private float ambientVolume = 1f;

		[Header("Fade Settings")]
		[SerializeField]
		private float defaultFadeTime = 2f;

		private Dictionary<string, ISoundPlayer> activeSounds;
		private HashSet<SoundEmitter> activeEmitters;
		private MusicSystem musicSystem;

		#region Setup

		protected override void Awake()
		{
			base.Awake();
			InitializeSystem();
		}

		private IEnumerator Start()
		{
			while (!RuntimeManager.HaveAllBanksLoaded)
				yield return null;

			yield return null;

			if (playMusicOnStart && startingMusic != null) 
				PlayStartingMusic();
		}


		public void InitializeSystem()
		{
			if (activeSounds != null) return;

			activeSounds = new Dictionary<string, ISoundPlayer>();
			activeEmitters = new HashSet<SoundEmitter>();
			musicSystem = new MusicSystem();

			masterBus = RuntimeManager.GetBus("bus:/");
			musicBus = RuntimeManager.GetBus("bus:/Music");
			sfxBus = RuntimeManager.GetBus("bus:/SFX");
			ambientBus = RuntimeManager.GetBus("bus:/Ambience");

			masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
			musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
			sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
			ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 1f);

			UpdateAllVolumes();
		}

		private async Awaitable PlayStartingMusic()
		{
			await PlayMusic(startingMusic);
			UpdateAllVolumes();
		}

		#endregion

		#region Volume Controls

		public enum AudioBus
		{
			Master,
			Music,
			SFX,
			Ambient
		}

		public void SetVolume(AudioBus bus, float value)
		{
			switch (bus)
			{
				case AudioBus.Master: MasterVolume = value; break;
				case AudioBus.Music: MusicVolume = value; break;
				case AudioBus.SFX: SFXVolume = value; break;
				case AudioBus.Ambient: AmbientVolume = value; break;
			}
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
			masterBus.setVolume(ConvertToFMODVolume(masterVolume));
			musicBus.setVolume(ConvertToFMODVolume(musicVolume));
			sfxBus.setVolume(ConvertToFMODVolume(sfxVolume));
			ambientBus.setVolume(ConvertToFMODVolume(ambientVolume));
		}

		#endregion

		#region Sound Playback

		private float ConvertToFMODVolume(float sliderValue)
		{
			if (sliderValue <= 0) return 0.0001f;

			return sliderValue * sliderValue;
		}

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

		public async Awaitable PlayMusic(SoundConfig music, float fadeTime = 2f)
		{
			await musicSystem.PlayMusic(music, fadeTime);
		}

		public void PauseMusic() => musicSystem.PauseMusic();
		public void ResumeMusic() => musicSystem.ResumeMusic();

		public async Awaitable StopMusic(float fadeTime = 2f)
		{
			await musicSystem.StopMusic(fadeTime);
		}
		public void RegisterEmitter(SoundEmitter emitter)
		{
			activeEmitters.Add(emitter);
		}

		public void StopMusicImmediate() => musicSystem.StopMusicImmediate();

		#endregion

		#region Emitter Management


		public void UnregisterEmitter(SoundEmitter emitter)
		{
			if (emitter != null) activeEmitters.Remove(emitter);
		}

		#endregion
  
		#region Cleanup

		public void StopAllSounds()
		{
			foreach (ISoundPlayer sound in activeSounds.Values) sound.Stop();
			foreach (SoundEmitter emitter in activeEmitters) emitter.Stop();
		}

		private void OnDestroy()
		{
			if (Instance != this) return;

			foreach (ISoundPlayer sound in activeSounds.Values) sound.Dispose();
			activeSounds.Clear();

			foreach (SoundEmitter emitter in activeEmitters.Where(emitter => emitter != null))
				Destroy(emitter.gameObject);
			activeEmitters.Clear();

			musicSystem?.StopMusicImmediate();
			musicSystem?.Dispose();
		}

		#endregion
	}
}