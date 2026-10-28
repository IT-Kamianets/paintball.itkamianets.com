using UnityEngine;

namespace ITKamianets.Paintball.Weapons
{
    /// <summary>Short generated shots until a sound bank is wired through 3d-unity-audio.</summary>
    public static class ShotAudio
    {
        static AudioClip _shot;
        static AudioClip _splat;

        public static void PlayShot(Vector3 position)
        {
            AudioSource.PlayClipAtPoint(Shot(), position, 0.35f);
        }

        public static void PlaySplat(Vector3 position)
        {
            AudioSource.PlayClipAtPoint(Splat(), position, 0.55f);
        }

        static AudioClip Shot()
        {
            if (_shot == null)
            {
                _shot = Tone("paintball-shot", 0.045f, 220f, 0.55f);
            }

            return _shot;
        }

        static AudioClip Splat()
        {
            if (_splat == null)
            {
                _splat = Tone("paintball-splat", 0.08f, 90f, 0.85f);
            }

            return _splat;
        }

        static AudioClip Tone(string name, float seconds, float frequency, float noise)
        {
            const int rate = 44100;
            var samples = Mathf.CeilToInt(rate * seconds);
            var data = new float[samples];
            var random = new System.Random(name.GetHashCode());
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)samples;
                var envelope = (1f - t) * (1f - t);
                var wave = Mathf.Sin(2f * Mathf.PI * frequency * i / rate);
                var hiss = (float)(random.NextDouble() * 2.0 - 1.0);
                data[i] = (wave * (1f - noise) + hiss * noise) * envelope * 0.8f;
            }

            var clip = AudioClip.Create(name, samples, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
