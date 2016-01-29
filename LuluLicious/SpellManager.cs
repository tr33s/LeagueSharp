using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace LuluLicious
{
    internal static class SpellManager
    {
        public static Spell Q;
        public static Spell PixQ;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _menu;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, 925);
            PixQ = new Spell(SpellSlot.Q, 925);
            W = new Spell(SpellSlot.W, 650);
            E = new Spell(SpellSlot.E, 650);
            R = new Spell(SpellSlot.R, 900);

            Q.SetSkillshot(0.25f, 60, 1450, false, SkillshotType.SkillshotLine);
            PixQ.SetSkillshot(0.25f, 60, 1450, false, SkillshotType.SkillshotLine);
        }

        public static void Initialize(Menu menu, Orbwalking.Orbwalker orbwalker)
        {
            _menu = menu;
            _orbwalker = orbwalker;
        }

        public static bool IsActive(this Spell spell, bool force = false)
        {
            if (force)
            {
                return true;
            }

            var name = spell.Slot + _orbwalker.ActiveMode.GetModeString();
            var item = _menu.Item(name);
            return item != null && item.IsActive();
        }
    }
}