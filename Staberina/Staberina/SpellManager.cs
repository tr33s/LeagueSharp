using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace Staberina
{
    internal static class SpellManager
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 375);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);

            Q.SetTargetted(400, 1400);
            E.SetTargetted(400, 1500);
        }

        public static void Initialize(Menu menu, Orbwalking.Orbwalker orbwalker)
        {
            _menu = menu;
            _orbwalker = orbwalker;
        }

        public static bool IsActive(this Spell spell)
        {
            var name = spell.Slot + _orbwalker.ActiveMode.GetModeString();
            var item = _menu.Item(name);
            return item != null && item.IsActive();
        }
    }
}