using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal static class Spells
    {
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell Ignite;

        static Spells()
        {
            Q = new Spell(SpellSlot.Q, 700);
            Q.SetTargetted(.401f, 2000);

            W = new Spell(SpellSlot.W, 600);
            W.SetSkillshot(.5f, 100, 2000, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 950);
            E.SetSkillshot(.25f, 70, 1600, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R);

            var ignite = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("summonerdot"));

            if (ignite.Slot != SpellSlot.Unknown)
            {
                Ignite = new Spell(ignite.Slot, 600);
                //Ignite.SetTargetted();
            }
        }
    }
}
