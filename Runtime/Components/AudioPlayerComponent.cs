using UnityEngine;

namespace ShoelaceStudios.AudioSystem
{
	public class AudioPlayerComponent : MonoBehaviour
	{
		[SerializeField] private SoundConfig soundConfig;
        
		private ISoundPlayer player;
        
		public void Play()
		{
			if (player == null && soundConfig != null)
			{
				player = AudioManager.Instance.CreateSound(soundConfig);
			}
            
			player?.Play();
		}
        
		public void Stop(bool fadeOut = true)
		{
			player?.Stop(fadeOut);
		}
        
		public void SetVolume(float volume)
		{
			player?.SetVolume(volume);
		}
        
		public void SetParameter(string paramName, float value)
		{
			player?.SetParameter(paramName, value);
		}
        
		public bool HasPlayer()
		{
			return player != null;
		}
        
		private void OnDestroy()
		{
			if (player == null) return;

			player.Stop(false);
			player.Dispose();
			player = null;
		}
	}
}