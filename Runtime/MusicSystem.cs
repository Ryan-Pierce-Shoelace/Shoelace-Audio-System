using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace ShoelaceStudios.AudioSystem
{
	public class MusicSystem : IDisposable
	{
		private EventInstance currentMusic;
		private EventInstance nextMusic;
		private SoundConfig currentConfig;
		private SoundConfig pendingConfig;
		private float pendingFadeTime;
		private bool isFading;
		private bool isPaused;
		private bool isValid = true;

		#region Playback

		public async Awaitable PlayMusic(SoundConfig config, float fadeTime = 2f)
		{
			if (!isValid || config == null) return;
			if (IsSameMusicPlaying(config)) return;

			if (isFading)
			{
				pendingConfig = config;
				pendingFadeTime = fadeTime;
				return;
			}

			if (currentMusic.isValid())
				await CrossfadeToNewMusic(config, fadeTime);
			else
				StartMusic(config);
		}

		public void PauseMusic()
		{
			if (!isValid || !currentMusic.isValid() || isPaused) return;

			currentMusic.setPaused(true);
			if (nextMusic.isValid()) nextMusic.setPaused(true);
			isPaused = true;
		}

		public void ResumeMusic()
		{
			if (!isValid || !currentMusic.isValid() || !isPaused) return;

			currentMusic.setPaused(false);
			if (nextMusic.isValid()) nextMusic.setPaused(false);
			isPaused = false;
		}

		public async Awaitable StopMusic(float fadeTime = 2f)
		{
			if (!isValid || !currentMusic.isValid() || isFading) return;

			try
			{
				isFading = true;
				float elapsed = 0f;

				while (elapsed < fadeTime && currentMusic.isValid())
				{
					elapsed += Time.deltaTime;
					currentMusic.setVolume(1f - Mathf.Clamp01(elapsed / fadeTime));
					await Awaitable.NextFrameAsync();
				}

				if (!currentMusic.isValid()) return;

				currentMusic.stop(STOP_MODE.IMMEDIATE);
				currentMusic.release();
				currentMusic = default;
				currentConfig = null;
				isPaused = false;
			}
			finally
			{
				isFading = false;
			}
		}

		public void StopMusicImmediate()
		{
			if (!isValid) return;

			if (currentMusic.isValid())
			{
				currentMusic.stop(STOP_MODE.IMMEDIATE);
				currentMusic.release();
				currentMusic = default;
			}

			if (nextMusic.isValid())
			{
				nextMusic.stop(STOP_MODE.IMMEDIATE);
				nextMusic.release();
				nextMusic = default;
			}

			currentConfig = null;
			pendingConfig = null;
			isFading = false;
			isPaused = false;
		}

		#endregion

		#region Crossfade

		private void StartMusic(SoundConfig config)
		{
			currentMusic = RuntimeManager.CreateInstance(config.EventRef);
			currentMusic.setVolume(1f);
			currentMusic.start();
			currentConfig = config;
		}

		private async Awaitable CrossfadeToNewMusic(SoundConfig config, float fadeTime)
		{
			try
			{
				isFading = true;

				nextMusic = RuntimeManager.CreateInstance(config.EventRef);
				nextMusic.setVolume(0f);
				nextMusic.start();

				await PerformCrossfade(fadeTime);

				SwapToNewMusic(config);
			}
			catch (Exception e)
			{
				Debug.LogError($"[MusicSystem] Crossfade error: {e.Message}");

				if (nextMusic.isValid())
				{
					nextMusic.stop(STOP_MODE.IMMEDIATE);
					nextMusic.release();
					nextMusic = default;
				}
			}
			finally
			{
				isFading = false;

				if (pendingConfig != null)
				{
					SoundConfig pending = pendingConfig;
					float pendingFade = pendingFadeTime;
					pendingConfig = null;
					await PlayMusic(pending, pendingFade);
				}
			}
		}

		private async Awaitable PerformCrossfade(float fadeTime)
		{
			float elapsed = 0f;
			while (elapsed < fadeTime)
			{
				elapsed += Time.deltaTime;
				float t = Mathf.Clamp01(elapsed / fadeTime);
				if (currentMusic.isValid()) currentMusic.setVolume(1f - t);
				if (nextMusic.isValid()) nextMusic.setVolume(t);
				await Awaitable.NextFrameAsync();
			}

			if (currentMusic.isValid()) currentMusic.setVolume(0f);
			if (nextMusic.isValid()) nextMusic.setVolume(1f);
		}

		private void SwapToNewMusic(SoundConfig config)
		{
			if (currentMusic.isValid())
			{
				currentMusic.stop(STOP_MODE.IMMEDIATE);
				currentMusic.release();
			}

			currentMusic = nextMusic;
			nextMusic = default;
			currentConfig = config;
		}

		#endregion

		#region Helpers

		private bool IsSameMusicPlaying(SoundConfig config) => currentConfig == config && currentMusic.isValid();

		#endregion

		#region Cleanup

		public void Dispose()
		{
			if (!isValid) return;

			try
			{
				if (currentMusic.isValid())
				{
					currentMusic.stop(STOP_MODE.IMMEDIATE);
					currentMusic.release();
				}

				if (nextMusic.isValid())
				{
					nextMusic.stop(STOP_MODE.IMMEDIATE);
					nextMusic.release();
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"[MusicSystem] Dispose error: {e.Message}");
			}
			finally
			{
				isValid = false;
			}
		}

		#endregion
	}
}