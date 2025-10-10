using System;

namespace ShoelaceStudios.AudioSystem
{
    public interface ISoundPlayer : IDisposable
    {
        void Play();
        void Stop(bool fadeOut = true);
        void SetVolume(float volume);
        void SetParameter(string name, float value);
    }
}
