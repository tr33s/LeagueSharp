using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Core.Events;
using LeagueSharp.SDK.Core.Wrappers.Spells.Database;

namespace AIOCaster
{
    internal class Program
    {
        public static List<DatabaseEntry> Spells = new List<DatabaseEntry>();
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static bool SDKOrbwalker
        {
            get { return Menu.Item("SDKOrbwalker").IsActive(); }
        }

        private static void Main(string[] args)
        {
            Load.OnLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(object sender, EventArgs e)
        {
            if (!Database.Spells.Any(s => s.ChampionName.Equals(Player.ChampionName)))
            {
                return;
            }

            Menu = new Menu("AIOCaster", "AIOCaster", true);

            //Menu.AddItem(new MenuItem("SDKOrbwalker", "Use SDK Orbwalker [RELOAD]").SetValue(false));

            //if (!SDKOrbwalker)
            {
                var orbMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
                Orbwalker = new Orbwalking.Orbwalker(orbMenu);
            }

            foreach (var spell in
                Database.Spells.Where(s => s.ChampionName.Equals(Player.ChampionName) && s.SpellType.IsSkillShot()))
            {
                Spells.Add(spell);
            }

            var spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));
            var addedSpells = new List<SpellSlot>();

            foreach (var spell in Spells.Where(s => !addedSpells.Contains(s.Slot)))
            {
                var s = spell.Slot.ToString();
                Console.WriteLine(s);
                var menu = spellMenu.AddSubMenu(new Menu(s, s));
                menu.AddItem(new MenuItem(s + "Combo", "Use in Combo", true).SetValue(true));
                menu.AddItem(new MenuItem(s + "Mixed", "Use in Harass", true).SetValue(true));
                addedSpells.Add(spell.Slot);
            }

            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnUpdate;
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || !Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            //var mode = SDKOrbwalker ? Variables.Orbwalker.GetActiveMode().ToString() : Orbwalker.ActiveMode.ToString();
            var mode = Orbwalker.ActiveMode.ToString();

            foreach (var spell in Spells.Where(s => Player.Spellbook.GetSpell(s.Slot).IsReady()))
            {
                if (!Menu.Item(spell.Slot + mode, true).IsActive())
                {
                    continue;
                }

                if (!Player.Spellbook.GetSpell(spell.Slot).Name.ToLower().Equals(spell.SpellName.ToLower()))
                {
                    continue;
                }

                var s = new Spell(spell.Slot, spell.Range);
                var collision = spell.CollisionObjects.Length > 1;
                var type = spell.SpellType.GetSkillshotType();

                s.SetSkillshot(spell.Delay, spell.Width, spell.MissileSpeed, collision, type);
                s.CastOnBestTarget();
            }
        }
    }
}