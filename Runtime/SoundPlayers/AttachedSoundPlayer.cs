using FMODUnity;
using UnityEngine;

namespace ShoelaceStudios.AudioSystem.SoundPlayers
{
	public class AttachedSoundPlayer : AbstractSoundPlayer
	{
		private readonly Transform attachedTransform;

		public AttachedSoundPlayer(SoundConfig config, Transform transform) : base(config)
		{
			attachedTransform = transform;
			RuntimeManager.AttachInstanceToGameObject(instance, transform);
		}
	}
}