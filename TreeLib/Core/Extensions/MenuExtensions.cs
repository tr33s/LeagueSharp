using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace TreeLib.Extensions
{
    public static class MenuExtensions
    {
        public static MenuItem AddList(this Menu menu,
            string name,
            string displayName,
            string[] list,
            int selectedIndex = 0)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new StringList(list, selectedIndex)));
        }

        public static MenuItem AddBool(this Menu menu, string name, string displayName, bool value = true)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(value));
        }

        public static MenuItem AddHitChance(this Menu menu, string name, string displayName, HitChance defaultHitChance)
        {
            return
                menu.AddItem(
                    new MenuItem(name, displayName).SetValue(
                        new StringList(new[] { "Low", "Medium", "High", "Very High" }, (int) defaultHitChance - 3)));
        }

        public static MenuItem AddSlider(this Menu menu,
            string name,
            string displayName,
            int value,
            int min = 0,
            int max = 100)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new Slider(value, min, max)));
        }

        public static Orbwalking.Orbwalker AddOrbwalker(this Menu menu)
        {
            var orbwalk = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            return new Orbwalking.Orbwalker(orbwalk);
        }

        public static MenuItem AddObject(this Menu menu, string name, string displayName, object value = null)
        {
            var i = menu.AddItem(new MenuItem(name, displayName));

            if (value != null)
            {
                i.SetValue(value);
            }

            return i;
        }

        public static MenuItem AddCircle(this Menu menu,
            string name,
            string displayName,
            Color color,
            float radius = 0,
            bool enabled = true)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new Circle(enabled, color, radius)));
        }

        public static MenuItem AddInfo(this Menu menu, string name, string text, SharpDX.Color fontColor)
        {
            return menu.AddItem(new MenuItem(name, text).SetFontStyle(FontStyle.Regular, fontColor));
        }

        public static Menu AddMenu(this Menu menu, string name, string displayName)
        {
            return menu.AddSubMenu(new Menu(displayName, name));
        }

        public static MenuItem AddKeyBind(this Menu menu,
            string name,
            string displayName,
            uint key,
            KeyBindType type = KeyBindType.Press,
            bool defaultValue = false)
        {
            return menu.AddItem(new MenuItem(name, displayName).SetValue(new KeyBind(key, type, defaultValue)));
        }

        public static Menu AddSpell(this Menu menu, SpellSlot spell, List<Orbwalking.OrbwalkingMode> modes)
        {
            var spellMenu = menu.AddMenu(spell.ToString(), spell.ToString());
            foreach (var mode in modes)
            {
                spellMenu.AddBool(mode.GetModeString() + spell, "Use in " + mode.GetModeString());
            }

            return spellMenu;
        }

        public static void SetSpellTooltip(this MenuItem item, string mode, string spell, SharpDX.Color color)
        {
            item.SetTooltip("Cast " + spell + " in " + mode + ".", color);
        }

        public static void SetDrawingTooltip(this MenuItem item, string spell, SharpDX.Color color)
        {
            item.SetTooltip("Draw " + spell + " range.", color);
        }

        public static void SetManaTooltip(this MenuItem item, SharpDX.Color color, string spell = null)
        {
            var text = spell == null ? "spells" : spell + ".";
            item.SetTooltip("Minimum mana to cast " + text, color);
        }
    }
}