using Shoelace.Events;
using UnityEngine;

namespace Shoelace.Audio.SOEvent
{
    [CreateAssetMenu(fileName = "New Void Event", menuName = "SO Architecture/SO Event/Sound Event")]
    public class SoundSOEvent : BaseSOEvent<SoundConfig>
    {
        public void Raise() => Raise(ScriptableObject.CreateInstance<SoundConfig>());
    }
}
