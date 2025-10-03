using FMOD.Studio;
using FMODUnity;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Shoelace.Audio.SoundPlayers
{
	public class AbstractSoundPlayer : ISoundPlayer
	{
		protected readonly EventInstance instance;
		protected readonly SoundConfig config;
		protected bool isValid = true;

		protected AbstractSoundPlayer(SoundConfig config)
		{
			this.config = config;
			instance = RuntimeManager.CreateInstance(config.EventRef);
			SetVolume(config.DefaultVolume);
		}

		public void Play()
		{
			if (!isValid) return;

			instance.start();
		}

		public void Stop(bool fadeOut = true)
		{
			if (!isValid) return;

			instance.stop(fadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
		}

		public void SetVolume(float volume)
		{
			if (!isValid) return;

			instance.setVolume(volume);
		}

		public void SetParameter(string name, float value)
		{
			if (!isValid) return;

			instance.setParameterByName(name, value);
		}

		public void Dispose()
		{
			if (!isValid) return;

			Stop(false);
			instance.release();
			isValid = false;
		}
	}
}