using ShoelaceStudios.SOAP.Events;
using UnityEngine;

namespace ShoelaceStudios.AudioSystem
{
    [CreateAssetMenu(fileName = "New Void Event", menuName = "🧩 SO Architecture/Events/Sound Event")]
    public class SoundSOEvent : BaseSOEvent<SoundConfig>
    {
        public void Raise() => Raise(CreateInstance<SoundConfig>());
    }
}
