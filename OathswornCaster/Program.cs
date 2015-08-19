#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace OathswornCaster
{
    internal class Program
    {
        public static Spell Oathsworn;
        public static Menu Menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (HeroManager.Allies.All(hero => hero.ChampionName != "Kalista"))
            {
                return;
            }

            Oathsworn = new Spell((SpellSlot) 0x3C, 300);
            Oathsworn.SetSkillshot(.862f, 20, float.MaxValue, false, SkillshotType.SkillshotLine);

            Menu = new Menu("OathswornCaster", "OathswornCaster", true);
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Health", "Min Health %").SetValue(new Slider(30)));
            Menu.AddItem(new MenuItem("Enemies", "Min Enemies").SetValue(new Slider(2, 1, 5)));
            Menu.AddToMainMenu();

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">Oathsworn Caster by </font><font color=\"#5C00A3\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Menu.Item("Enabled").GetValue<KeyBind>().Active ||
                ObjectManager.Player.HealthPercent < Menu.Item("Health").GetValue<Slider>().Value)
            {
                return;
            }

            if (Oathsworn.Instance == null || Oathsworn.Instance.Name == null ||
                !Oathsworn.Instance.Name.Equals("KalistaRAllyDash"))
            {
                return;
            }

            var targ = TargetSelector.GetTarget(350, TargetSelector.DamageType.Magical);

            if (!targ.IsValidTarget() ||
                targ.ServerPosition.CountEnemiesInRange(200) < Menu.Item("Enemies").GetValue<Slider>().Value)
            {
                return;
            }

            Oathsworn.Cast(targ.ServerPosition);
        }
    }
}