using FMODUnity;
using UnityEngine;

namespace ShoelaceStudios.AudioSystem
{
	[RequireComponent(typeof(StudioEventEmitter))]
	public class SoundEmitter : MonoBehaviour
	{
		[SerializeField] private SoundConfig soundConfig;
		[SerializeField] private bool playOnStart;
		[SerializeField] private bool allowFadeout = true;

		private StudioEventEmitter studioEventEmitter;

		private void Awake()
		{
			studioEventEmitter = GetComponent<StudioEventEmitter>();
			SetupEventEmitter();
		}

		private void Start()
		{
			AudioManager.Instance.RegisterEmitter(this);

			if (playOnStart) Play();
		}

		private void SetupEventEmitter()
		{
			if (soundConfig == null) return;

			studioEventEmitter.EventReference = soundConfig.EventRef;
			studioEventEmitter.AllowFadeout = allowFadeout;
		}

		private void OnValidate()
		{
			if (!studioEventEmitter) studioEventEmitter = GetComponent<StudioEventEmitter>();
			SetupEventEmitter();
		}

		public void Play()
		{
			if (soundConfig == null) return;

			studioEventEmitter.Play();
			SetVolume(soundConfig.DefaultVolume);
		}

		public void Stop()
		{
			studioEventEmitter.Stop();
		}

		public void SetVolume(float volume)
		{
			if (!studioEventEmitter.EventInstance.isValid()) return;

			studioEventEmitter.EventInstance.setVolume(volume);
		}

		public void SetParameter(string paramName, float value)
		{
			studioEventEmitter.SetParameter(paramName, value);
		}

		private void OnDestroy()
		{
			AudioManager.Instance?.UnregisterEmitter(this);
		}
	}
}