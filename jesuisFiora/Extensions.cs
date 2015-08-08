using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace jesuisFiora
{
    internal static class Extensions
    {
        public static int GetVitalCount(this Obj_AI_Base unit)
        {
            return unit.Buffs.Count(buff => buff.Name.Equals("FIORAVITAL"));
        }

        public static bool IsActive(this Orbwalking.OrbwalkingMode mode)
        {
            return mode != Orbwalking.OrbwalkingMode.None;
        }

        public static SpellDataInst[] GetMainSpells(this Spellbook spellbook)
        {
            return new[]
            {
                spellbook.GetSpell(SpellSlot.Q), spellbook.GetSpell(SpellSlot.W), spellbook.GetSpell(SpellSlot.E),
                spellbook.GetSpell(SpellSlot.R)
            };
        }

        public static HitChance GetHitChance(this MenuItem item)
        {
            return (HitChance) item.GetValue<StringList>().SelectedIndex + 3;
        }

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

        public static void AddTargetSelector(this Menu menu)
        {
            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            menu.AddSubMenu(ts);
        }

        public static void AddObject(this Menu menu, string name, string displayName, object value = null)
        {
            var i = menu.AddItem(new MenuItem(name, displayName));

            if (value != null)
            {
                i.SetValue(value);
            }
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
    }
}