using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace TentacleBabeIllaoi
{
    internal static class SpellManager
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, 850);
            Q.SetSkillshot(.75f, 100, float.MaxValue, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 950);
            E.SetSkillshot(.25f, 50, 1900, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 450);
        }

        public static bool IsActive(this Spell spell)
        {
            var name = spell.Slot + Program.Orbwalker.ActiveMode.GetModeString();
            var item = Program.Menu.Item(name);
            return item != null && item.IsActive();
        }
    }
}