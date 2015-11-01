using System;
using System.Linq;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace TreeLib.SpellData
{
    internal static class Config
    {
        #region Public Methods and Operators

        public static void CreateMenu()
        {
            Menu = new Menu("Evade Skillshot", "Evade");
            foreach (var spell in
                SpellDatabase.Spells.Where(
                    i =>
                        HeroManager.Enemies.Any(
                            a =>
                                string.Equals(
                                    a.ChampionName, i.ChampionName, StringComparison.InvariantCultureIgnoreCase))))
            {
                var subMenu = new Menu(string.Format("{0} ({1})", spell.SpellName, spell.Slot), spell.SpellName);
                subMenu.AddSlider("DangerLevel", "Danger Level", spell.DangerValue, 1, 5);
                subMenu.AddBool("IsDangerous", "Is Dangerous", spell.IsDangerous);
                subMenu.AddBool("DisableFoW", "Disable FoW Dodging", false);
                subMenu.AddBool("Draw", "Draw", false);
                subMenu.AddBool("Enabled", "Enabled", !spell.DisabledByDefault);
                Menu.SubMenu(spell.ChampionName.ToLowerInvariant()).AddSubMenu(subMenu);
                Menu.AddBool("DrawStatus", "Draw Evade Status");
                Menu.AddKeyBind("Enabled", "Enabled", 'K', KeyBindType.Toggle);
                Menu.AddKeyBind("OnlyDangerous", "Dodge Only Dangerous", 32);
            }
        }

        #endregion

        #region Constants

        public const int DiagonalEvadePointsCount = 7;

        public const int DiagonalEvadePointsStep = 20;

        public const int EvadingFirstTimeOffset = 250;

        public const int EvadingSecondTimeOffset = 80;

        public const int ExtraEvadeDistance = 15;

        public const int GridSize = 10;

        public const int SkillShotsExtraRadius = 9;

        public const int SkillShotsExtraRange = 20;

        public static Menu Menu;

        #endregion
    }
}