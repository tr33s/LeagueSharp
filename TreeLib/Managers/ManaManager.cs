using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;
using TreeLib.Objects;
using Color = SharpDX.Color;

namespace TreeLib.Managers
{
    public static class ManaManager
    {
        public enum ManaMode
        {
            Combo,
            Harass,
            Farm,
            None
        }

        private static Menu _menu;

        private static readonly Dictionary<ManaMode, Dictionary<SpellSlot, int>> ManaDictionary =
            new Dictionary<ManaMode, Dictionary<SpellSlot, int>>();

        private static ManaMode CurrentMode
        {
            get
            {
                switch (Champion.Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        return ManaMode.Combo;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        return ManaMode.Harass;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        return ManaMode.Farm;
                }

                return ManaMode.None;
            }
        }

        public static void Initialize(Menu menu)
        {
            _menu = menu.AddMenu("ManaManager", "Mana Manager");
            _menu.SetFontStyle(FontStyle.Regular, Color.Cyan);
            _menu.AddBool("Enabled", "Enabled", false);
        }

        public static void SetManaCondition(this Spell spell, ManaMode mode, int value)
        {
            if (!ManaDictionary.ContainsKey(mode))
            {
                ManaDictionary.Add(mode, new Dictionary<SpellSlot, int>());
            }

            ManaDictionary[mode].Add(spell.Slot, value);
            var m = mode.ToString();

            if (!_menu.SubMenu(m).Items.Any())
            {
                _menu.SubMenu(m).AddBool(m + "Enabled", "Enabled in " + m);
                _menu.SubMenu(m).AddInfo(m + "Info", "-- Spells -- ", Color.AliceBlue);
            }

            var item = _menu.SubMenu(m)
                .AddSlider(ObjectManager.Player.ChampionName + spell.Slot + "Mana", spell.Slot + " Mana Percent", value);
            item.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs args)
                {
                    ManaDictionary[mode][spell.Slot] = args.GetNewValue<Slider>().Value;
                };
        }

        public static bool HasManaCondition(this Spell spell)
        {
            if (!_menu.Item("Enabled").IsActive())
            {
                return false;
            }

            var mode = CurrentMode;

            if (mode == ManaMode.None || !ManaDictionary.ContainsKey(mode) || !_menu.Item(mode + "Enabled").IsActive())
            {
                return false;
            }

            var currentMode = ManaDictionary[mode];

            if (!currentMode.ContainsKey(spell.Slot))
            {
                return false;
            }

            return ObjectManager.Player.ManaPercent < currentMode[spell.Slot];
        }
    }
}