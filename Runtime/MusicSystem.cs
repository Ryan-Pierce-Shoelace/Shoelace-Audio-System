using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace ShoelaceStudios.Audio
{
    public class MusicSystem : IDisposable
    {
        public float Volume 
        { 
            get => volume;
            set 
            {
                volume = value;
                if (currentMusic.isValid())
                    currentMusic.setVolume(volume);
            }
        }
      
        
        private float volume = 1f;
        
        private EventInstance currentMusic;
        private EventInstance nextMusic;
        private SoundConfig currentConfig;
        private bool isFading;
        private bool isValid = true;
        
        private const float UPDATE_INTERVAL = 0.02f;

        public async void PlayMusic(SoundConfig config, float fadeTime)
        {
            if (!isValid || isFading) return;

            if (IsSameMusicPlaying(config))
                return;

            if (currentMusic.isValid())
            {
                await CrossfadeToNewMusic(config, fadeTime);
            }
            else
            {
                currentMusic = RuntimeManager.CreateInstance(config.EventRef);
                currentMusic.setVolume(1f);
                currentMusic.start();
                currentConfig = config;
            }
        }

        private bool IsSameMusicPlaying(SoundConfig config)
        {
            return currentConfig == config && currentMusic.isValid();
        }

        private async Awaitable CrossfadeToNewMusic(SoundConfig config, float fadeTime)
        {
            try 
            {
                isFading = true;

                nextMusic = RuntimeManager.CreateInstance(config.EventRef);
                nextMusic.setVolume(0);
                nextMusic.start();

                await PerformCrossfade(fadeTime);

                SwapToNewMusic(config);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during music crossfade: {e.Message}");
                if (nextMusic.isValid())
                {
                    nextMusic.stop(STOP_MODE.IMMEDIATE);
                    nextMusic.release();
                }
            }
            finally
            {
                isFading = false;
            }
        }

        private async Awaitable PerformCrossfade(float fadeTime)
        {
            float elapsed = 0;

            while (elapsed < fadeTime)
            {
                float t = elapsed / fadeTime;
                if (currentMusic.isValid()) currentMusic.setVolume(1 - t);
                if (nextMusic.isValid()) nextMusic.setVolume(t);

                await Awaitable.WaitForSecondsAsync(.02f);
                elapsed += UPDATE_INTERVAL;
            }
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


        public async void StopMusic(float fadeTime)
        {
            if (!isValid || !currentMusic.isValid() || isFading) return;

            try
            {
                isFading = true;
                float elapsed = 0;

                while (elapsed < fadeTime && currentMusic.isValid())
                {
                    currentMusic.setVolume(1 - (elapsed / fadeTime));
                    await Awaitable.WaitForSecondsAsync(.02f);
                    elapsed += UPDATE_INTERVAL;
                }

                if (!currentMusic.isValid()) return;
                
                currentMusic.stop(STOP_MODE.IMMEDIATE);
                currentMusic.release();
                currentConfig = null;
            }
            finally
            {
                isFading = false;
            }
        }

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
                Debug.LogError($"Error disposing music system: {e.Message}");
            }
            finally
            {
                isValid = false;
            }
        }
    }
}