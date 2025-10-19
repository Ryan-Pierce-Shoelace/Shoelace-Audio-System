using UnityEngine;

namespace ShoelaceStudios.AudioSystem
{
	public class AudioPlayerComponent : MonoBehaviour
	{
		private ISoundPlayer currentPlayer;
        
		public void PlaySound(SoundConfig config)
		{
			StopSound(false);
            
			if (config != null)
			{
				currentPlayer = AudioManager.Instance.CreateSound(config);
				currentPlayer.Play();
			}
		}
        
		public void StopSound(bool fadeOut = true)
		{
			if (currentPlayer != null)
			{
				currentPlayer.Stop(fadeOut);
				currentPlayer.Dispose();
				currentPlayer = null;
			}
		}
        
		public void SetVolume(float volume)
		{
			currentPlayer?.SetVolume(volume);
		}
        
		public bool IsPlaying()
		{
			return currentPlayer != null;
		}
        
		private void OnDestroy()
		{
			StopSound(false);
		}
	}
}