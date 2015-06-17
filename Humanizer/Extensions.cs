using LeagueSharp;

namespace Humanizer
{
    internal static class Extensions
    {
        public static bool IsMainSpell(this SpellSlot slot)
        {
            return slot == SpellSlot.Q || slot == SpellSlot.W || slot == SpellSlot.E || slot == SpellSlot.R;
        }
    }
}