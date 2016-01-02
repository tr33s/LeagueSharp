using LeagueSharp.Common;
using TreeLib.Extensions;
using TreeLib.Objects;

namespace EatShitSivir
{
    internal static class Utility
    {
        public static bool IsActive(this Spell spell)
        {
            var name = Champion.Orbwalker.ActiveMode.GetModeString() + spell.Slot;
            var item = Champion.Menu.Item(name);
            return item != null && item.IsActive();
        }
    }
}