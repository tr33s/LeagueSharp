using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;
using TreeLib.Extensions;

namespace TreeLib.Managers
{
    public static class SpellManager
    {
        public static Spell Ignite;
        public static Spell Smite;
        internal static Menu Menu;

        internal static void Initialize()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Summoners", "Summoners");
            Menu.AddBool("SmiteManagerEnabled", "Load Smite Manager");
            Menu.AddBool("IgniteManagerEnabled", "Load Ignite Manager");
            Bootstrap.Menu.AddSubMenu(Menu);

            var smite = ObjectManager.Player.Spellbook.Spells.FirstOrDefault(h => h.Name.ToLower().Contains("smite"));

            if (smite != null && !smite.Slot.Equals(SpellSlot.Unknown))
            {
                Smite = new Spell(smite.Slot, 500);
                Smite.SetTargetted(.333f, 20);

                if (Menu.Item("SmiteManagerEnabled").IsActive())
                {
                    SmiteManager.Initialize();
                }
            }

            var igniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

            if (!igniteSlot.Equals(SpellSlot.Unknown))
            {
                Ignite = new Spell(igniteSlot, 600);
                Ignite.SetTargetted(.172f, 20);

                if (Menu.Item("IgniteManagerEnabled").IsActive())
                {
                    IgniteManager.Initialize();
                }
            }
        }
    }
}