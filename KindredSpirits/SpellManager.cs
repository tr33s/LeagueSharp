using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace KindredSpirits
{
    internal static class SpellManager
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;
        public static Spell Smite;

        static SpellManager()
        {
            Q = new Spell(SpellSlot.Q, 340 + 500);
            Q.SetSkillshot(.135f, 80, 1600, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 800);
            W.SetSkillshot(.38f, 0, 0, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 500);
            E.SetTargetted(0, 0);

            R = new Spell(SpellSlot.R, 500);
            R.SetTargetted(.672f, 1200);

            var igniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

            if (!igniteSlot.Equals(SpellSlot.Unknown))
            {
                Ignite = new Spell(igniteSlot, 600);
                Ignite.SetTargetted(.172f, 20);
            }

            var smite = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(h => h.Name.ToLower().Contains("smite"));

            if (smite != null && !smite.Slot.Equals(SpellSlot.Unknown))
            {
                Smite = new Spell(smite.Slot, 500);
                Smite.SetTargetted(.333f, 20);
                SmiteManager.Initialize();
            }
        }
    }
}