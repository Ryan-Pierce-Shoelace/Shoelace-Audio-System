using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace ShoelaceStudios.Audio
{
	[CreateAssetMenu(fileName = "New Sound Config", menuName = "Audio/Sound Config")]
	public class SoundConfig : ScriptableObject
	{
		[SerializeField] private EventReference eventReference;
		[SerializeField] private bool is3D;
		[SerializeField] private float defaultVolume = 1f;

		[SerializeField] 
		private List<ParameterConfig> parameters = new();

		public EventReference EventRef => eventReference;
		public bool Is3D => is3D;
		public float DefaultVolume => defaultVolume;
		public IReadOnlyList<ParameterConfig> Parameters => parameters;
		
		[Serializable]
		public struct ParameterConfig
		{
			public string Name;
			public PARAMETER_ID ID;
			public float DefaultValue;
			public float MinValue;
			public float MaxValue;
		}


		public void LoadParameterDescriptions()
		{
			if (eventReference.IsNull) return;

			EventDescription eventDescription = RuntimeManager.GetEventDescription(eventReference);
			if (!eventDescription.isValid()) return;

			eventDescription.getParameterDescriptionCount(out int paramCount);
			parameters.Clear();

			for (int i = 0; i < paramCount; i++)
			{
				eventDescription.getParameterDescriptionByIndex(i, out PARAMETER_DESCRIPTION paramDesc);
				parameters.Add(new ParameterConfig
				{
					Name = paramDesc.name,
					ID = paramDesc.id,
					DefaultValue = paramDesc.defaultvalue,
					MinValue = paramDesc.minimum,
					MaxValue = paramDesc.maximum
				});
			}
		}
	}
}