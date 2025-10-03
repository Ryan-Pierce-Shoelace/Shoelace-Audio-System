using System;

namespace Shoelace.Audio.SoundPlayers
{
    public interface ISoundPlayer : IDisposable
    {
        void Play();
        void Stop(bool fadeOut = true);
        void SetVolume(float volume);
        void SetParameter(string name, float value);
    }
}
