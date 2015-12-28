using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK.Core.Wrappers.SpellDatabase;

namespace AIOCaster
{
    internal class Program
    {
        public static Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>();
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!SpellDatabase.Spells.Any(s => s.ChampionName.Equals(Player.ChampionName)))
            {
                return;
            }

            Menu = new Menu("AIOCaster", "AIOCaster", true);

            var orbMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbMenu);

            foreach (var spell in
                SpellDatabase.Spells.Where(s => s.ChampionName.Equals(Player.ChampionName) && s.SpellType.IsSkillShot())
                )
            {
                var s = new Spell(spell.Slot, spell.Range);
                var collision = spell.CollisionObjects.Length > 1;
                var type = spell.SpellType.GetSkillshotType();

                s.SetSkillshot(spell.Delay, spell.Width, spell.MissileSpeed, collision, type);
                Spells.Add(s.Slot, s);
            }

            var spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));

            foreach (var spell in Spells)
            {
                var s = spell.Key.ToString();
                var menu = spellMenu.AddSubMenu(new Menu(s, s));
                menu.AddItem(new MenuItem(s + "Combo", "Use in Combo", true).SetValue(true));
                menu.AddItem(new MenuItem(s + "Mixed", "Use in Harass", true).SetValue(true));
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

            var mode = Orbwalker.ActiveMode.ToString();

            foreach (var spell in Spells.Where(s => s.Value.IsReady()))
            {
                var active = Menu.Item(spell.Key + mode, true) != null && Menu.Item(spell.Key + mode, true).IsActive();

                if (!active)
                {
                    return;
                }

                spell.Value.CastOnBestTarget();
            }
        }
    }
}