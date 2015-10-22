using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace TUrgot
{
    internal static class Extensions
    {
        public static bool HasUrgotEBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("urgotcorrosivedebuff");
        }

        public static string ToHexString(this Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }

        public static bool IsActive(this Orbwalking.OrbwalkingMode mode)
        {
            return mode != Orbwalking.OrbwalkingMode.None;
        }

        public static bool IsComboMode(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.Combo) || mode.Equals(Orbwalking.OrbwalkingMode.Mixed);
        }

        public static bool IsFarmMode(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.LastHit) || mode.Equals(Orbwalking.OrbwalkingMode.LaneClear);
        }

        public static SpellSlot GetSpellSlot(this Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var instance = sender.Spellbook.Spells.FirstOrDefault(spell => spell.Name.Equals(args.SData.Name));
            return instance == null ? SpellSlot.Unknown : instance.Slot;
        }

        public static SpellDataInst[] GetMainSpells(this Spellbook spellbook)
        {
            return new[]
            {
                spellbook.GetSpell(SpellSlot.Q), spellbook.GetSpell(SpellSlot.W), spellbook.GetSpell(SpellSlot.E),
                spellbook.GetSpell(SpellSlot.R)
            };
        }

        public static bool IsSkillShot(this SpellDataTargetType type)
        {
            return type.Equals(SpellDataTargetType.Location) || type.Equals(SpellDataTargetType.Location2) ||
                   type.Equals(SpellDataTargetType.LocationVector);
        }

        public static bool IsTargeted(this SpellDataTargetType type)
        {
            return type.Equals(SpellDataTargetType.Unit) || type.Equals(SpellDataTargetType.SelfAndUnit);
        }

        public static string GetModeString(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.Mixed) ? "Harass" : mode.ToString();
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

        public static ColorBGRA ToBGRA(this Color color)
        {
            return new ColorBGRA(color.R, color.G, color.B, color.A);
        }
    }
}