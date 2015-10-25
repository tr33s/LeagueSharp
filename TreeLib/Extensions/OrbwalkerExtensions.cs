using LeagueSharp.Common;

namespace TreeLib.Extensions
{
    public static class OrbwalkerExtensions
    {
        public static bool IsActive(this Orbwalking.OrbwalkingMode mode)
        {
            return mode != Orbwalking.OrbwalkingMode.None;
        }

        public static bool IsComboMode(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.Combo) || mode.Equals(Orbwalking.OrbwalkingMode.Mixed);
        }

        public static bool IsFarmMode(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.LastHit) || mode.Equals(Orbwalking.OrbwalkingMode.LaneClear);
        }

        public static string GetModeString(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.Mixed) ? "Harass" : mode.ToString();
        }
    }
}