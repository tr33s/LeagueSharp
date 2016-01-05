using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace PopBlanc
{
    internal static class SpellManager
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static Orbwalking.Orbwalker _orbwalker;
        private static Menu _menu;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, 700);
            Q.SetTargetted(.401f, 2000);

            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 950);
            E.SetSkillshot(.25f, 70, 1600, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R);
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

            var name = _orbwalker.ActiveMode.GetModeString() + spell.Slot;
            var item = _menu.Item(name);
            return item != null && item.IsActive();
        }

        public static bool IsFirstW(this Spell spell)
        {
            return spell.Instance.ToggleState.Equals(1);
        }

        public static SpellSlot GetSpellSlot(this Spell spell)
        {
            switch (spell.Instance.Name)
            {
                case "LeblancChaosOrbM":
                    return SpellSlot.Q;
                case "LeblancSlideM":
                    return SpellSlot.W;
                case "LeblancSoulShackleM":
                    return SpellSlot.E;
                default:
                    return SpellSlot.R;
            }
        }

        public static void UpdateUltimate()
        {
            switch (R.GetSpellSlot())
            {
                case SpellSlot.Q:
                    R = new Spell(SpellSlot.R, 700);
                    R.SetTargetted(.401f, 2000);
                    return;
                case SpellSlot.W:
                    R = new Spell(SpellSlot.R, 600);
                    R.SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);
                    return;
                case SpellSlot.E:
                    R = new Spell(SpellSlot.R, 950);
                    R.SetSkillshot(.366f, 70, 1600, true, SkillshotType.SkillshotLine);
                    return;
            }
        }
    }
}