using ShoelaceStudios.SOAP.Events;
using UnityEngine;

namespace ShoelaceStudios.Audio.SOEvent
{
    [CreateAssetMenu(fileName = "New Void Event", menuName = "SO Architecture/SO Event/Sound Event")]
    public class SoundSOEvent : BaseSOEvent<SoundConfig>
    {
        public void Raise() => Raise(ScriptableObject.CreateInstance<SoundConfig>());
    }
}
