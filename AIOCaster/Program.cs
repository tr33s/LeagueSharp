using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.Core.UI.IMenu;
using LeagueSharp.SDK.Core.UI.IMenu.Values;
using LeagueSharp.SDK.MoreLinq;
using SharpDX;

namespace AIOCaster
{
    internal class Program
    {
        public static List<SpellDatabaseEntry> Spells = new List<SpellDatabaseEntry>();
        public static Menu Menu;
        public static Menu SpellMenu;

        public static Obj_AI_Hero Player
        {
            get { return GameObjects.Player; }
        }

        private static void Main(string[] args)
        {
            Events.OnLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(object sender, EventArgs e)
        {
            if (!SpellDatabase.Spells.Any(s => s.ChampionName.Equals(Player.ChampionName)))
            {
                return;
            }

            Menu = new Menu("AIOCaster", "AIOCaster", true);

            SpellMenu = Menu.Add(new Menu("Spells", "Spells"));
            Menu.Add(new MenuBool("Dashes", "Load Dashes [RELOAD]"));
            Menu.Add(new MenuBool("KS", "Killsteal", true));
            Menu.Add(new MenuBool("DisableDraw", "Disable Drawings"));

            foreach (var spell in
                SpellDatabase.Spells.Where(
                    s =>
                        s.ChampionName.Equals(Player.ChampionName) && s.CastType.IsSkillShot() &&
                        (Menu.GetValue<MenuBool>("Dashes").Value || !s.SpellTags.Contains(SpellTags.Dash))))
            {
                Spells.Add(spell);
            }

            foreach (var spell in Spells.DistinctBy(s => s.Slot))
            {
                var s = spell.Slot.ToString();
                var menu = SpellMenu.Add(new Menu(s, s));
                menu.Add(new MenuBool(s + "Combo" + Player.ChampionName, "Use in Combo", true));
                menu.Add(new MenuBool(s + "Hybrid" + Player.ChampionName, "Use in Hybrid", true));
                if (spell.Radius < 5000)
                {
                    menu.Add(new MenuBool(s + "Draw" + Player.ChampionName, "Draw Range"));
                    menu.Add(new MenuColor(s + "Color" + Player.ChampionName, "Color", Color.Red));
                }
            }

            Menu.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Menu.GetValue<MenuBool>("DisableDraw").Value)
            {
                return;
            }

            foreach (var spell in Spells.Where(s => s.Range < 5000))
            {
                if (
                    Menu["Spells"][spell.Slot.ToString()].GetValue<MenuBool>(spell.Slot + "Draw" + Player.ChampionName)
                        .Value)
                {
                    var color =
                        Menu["Spells"][spell.Slot.ToString()].GetValue<MenuColor>(
                            spell.Slot + "Color" + Player.ChampionName).Color;
                    Utility.RenderCircle(Player.Position, spell.Range, color);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Menu.GetValue<MenuBool>("KS").Value && CastSequence("KS"))
            {
                return;
            }

            if (Variables.Orbwalker.ActiveMode.IsComboMode() && CastSequence()) {}
        }

        private static bool CastSequence(string mode = null)
        {
            Console.WriteLine("Cast Sequence");
            mode = string.IsNullOrEmpty(mode) ? Variables.Orbwalker.ActiveMode.ToString() : mode;

            foreach (var spell in Spells.Where(s => s.Slot.IsReady()))
            {
                try
                {
                    if (mode != "KS" &&
                        !Menu["Spells"][spell.Slot.ToString()].GetValue<MenuBool>(
                            spell.Slot + mode + Player.ChampionName).Value)
                    {
                        continue;
                    }

                    if (!spell.IsCurrentSpell())
                    {
                        continue;
                    }

                    var s = spell.CreateSpell();
                    var targ = s.GetTarget();

                    if (!targ.IsValidTarget())
                    {
                        continue;
                    }

                    if ((mode != "KS" || s.CanKill(targ)) && s.Cast(targ) == CastStates.SuccessfullyCasted)
                    {
                        return true;
                    }
                }
                catch {}
            }

            return false;
        }
    }
}