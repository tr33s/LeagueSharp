using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Babehri
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
            Q = new Spell(SpellSlot.Q, 880);
            Q.SetSkillshot(.25f, 100, 2500, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 630);

            E = new Spell(SpellSlot.E, 975);
            E.SetSkillshot(.25f, 60, 1550, true, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 450);

            var ignite = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(spell => spell.Name.Equals("summonerdot"));

            if (ignite != null && ignite.Slot != SpellSlot.Unknown)
            {
                Ignite = new Spell(ignite.Slot, 600);
                //Ignite.SetTargetted();
            }
        }

        public static bool IsActive(this Spell spell)
        {
            var mode = Program.Orbwalker.ActiveMode.GetModeString();
            return Program.Menu.Item(mode + spell.Slot).IsActive();
        }
    }
}