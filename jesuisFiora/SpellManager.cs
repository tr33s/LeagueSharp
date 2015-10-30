using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace jesuisFiora
{
    internal static class SpellManager
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;
        public static float QSkillshotRange = 400;
        public static float QCircleRadius = 350;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, QSkillshotRange + QCircleRadius);
            Q.SetSkillshot(.25f, 0, 500, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 750);
            W.SetSkillshot(0.5f, 70, 3200, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E);
            E.SetTargetted(0f, 0f);

            R = new Spell(SpellSlot.R, 500);
            R.SetTargetted(.066f, 500);

            var igniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

            if (!igniteSlot.Equals(SpellSlot.Unknown))
            {
                Ignite = new Spell(igniteSlot, 600);
                Ignite.SetTargetted(.172f, 20);
            }
        }

        public static bool IsActive(this Spell spell)
        {
            var name = spell.Slot + Program.Orbwalker.ActiveMode.GetModeString();
            var item = Program.Menu.Item(name);
            return item != null && item.IsActive();
        }
    }
}