using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;
using TreeLib.Extensions;

namespace AntiEvadeInterrupter
{
    internal class Program
    {
        public static Menu Menu;
        public static InterruptableSpell Spell;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Interrupter.Spells.Any(spell => spell.ChampionName.Equals(ObjectManager.Player.ChampionName)))
            {
                return;
            }

            Bootstrap.Initialize();

            Menu = new Menu("AntiEvadeInterrupter", "AntiEvadeInterrupter", true);
            Menu.AddBool("Enabled", "Enabled");
            Menu.Item("Enabled").SetTooltip("Stops evade from interrupting important spells.");
            Menu.AddToMainMenu();

            Spell =
                Interrupter.Spells.FirstOrDefault(spell => spell.ChampionName.Equals(ObjectManager.Player.ChampionName));

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (EvadeDisabler.EvadeDisabled && !ObjectManager.Player.IsChannelingImportantSpell())
            {
                EvadeDisabler.EnableEvade();
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && sender.IsValid && sender.IsMe && args.Slot.Equals(Spell.Slot))
            {
                EvadeDisabler.DisableEvade();
            }
        }
    }
}