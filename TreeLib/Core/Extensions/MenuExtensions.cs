using System.Drawing;
using LeagueSharp.Common;

namespace TreeLib.Extensions
{
    public static class MenuExtensions
    {
        public static void AddList(this Menu menu, string name, string displayName, string[] list, int selectedIndex = 0)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new StringList(list, selectedIndex)));
        }

        public static void AddBool(this Menu menu, string name, string displayName, bool value = true)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(value));
        }

        public static void AddHitChance(this Menu menu, string name, string displayName, HitChance defaultHitChance)
        {
            menu.AddItem(
                new MenuItem(name, displayName).SetValue(
                    new StringList((new[] { "Low", "Medium", "High", "Very High" }), (int) defaultHitChance - 3)));
        }

        public static void AddSlider(this Menu menu,
            string name,
            string displayName,
            int value,
            int min = 0,
            int max = 100)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new Slider(value, min, max)));
        }

        public static Orbwalking.Orbwalker AddOrbwalker(this Menu menu)
        {
            var orbwalk = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            return new Orbwalking.Orbwalker(orbwalk);
        }

        public static void AddObject(this Menu menu, string name, string displayName, object value = null)
        {
            var i = menu.AddItem(new MenuItem(name, displayName));

            if (value != null)
            {
                i.SetValue(value);
            }
        }

        public static void AddCircle(this Menu menu,
            string name,
            string displayName,
            Color color,
            float radius = 0,
            bool enabled = true)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new Circle(enabled, color, radius)));
        }

        public static void AddInfo(this Menu menu, string name, string text, SharpDX.Color fontColor)
        {
            menu.AddItem(new MenuItem(name, text).SetFontStyle(FontStyle.Regular, fontColor));
        }

        public static Menu AddMenu(this Menu menu, string name, string displayName)
        {
            return menu.AddSubMenu(new Menu(displayName, name));
        }

        public static void AddKeyBind(this Menu menu,
            string name,
            string displayName,
            uint key,
            KeyBindType type = KeyBindType.Press,
            bool defaultValue = false)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(new KeyBind(key, type, defaultValue)));
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
            var text = spell == null ? "spells" : (spell + ".");
            item.SetTooltip("Minimum mana to cast " + text, color);
        }
    }
}