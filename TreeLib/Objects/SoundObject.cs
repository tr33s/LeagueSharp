using System.IO;
using System.Media;
using LeagueSharp.Common;

namespace TreeLib.Objects
{
    public class SoundObject
    {
        public static float LastPlayed;
        private static SoundPlayer _sound;

        public SoundObject(Stream sound)
        {
            LastPlayed = 0;
            _sound = new SoundPlayer(sound);
        }

        public void Play()
        {
            if (Utils.TickCount - LastPlayed < 1500)
            {
                return;
            }
            _sound.Play();
            LastPlayed = Utils.TickCount;
        }
    }
}