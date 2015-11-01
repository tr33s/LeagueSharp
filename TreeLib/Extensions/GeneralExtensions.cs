using LeagueSharp.Common;

namespace TreeLib.Extensions
{
    internal static class GeneralExtensions
    {
        public static int TimeSince(this int time)
        {
            return Utils.TickCount - time;
        }
    }
}