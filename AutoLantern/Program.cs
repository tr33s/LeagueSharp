#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace AutoLantern
{
    internal class Program
    {
        public static Menu Menu;
        public static Render.Text LanternText;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static SpellSlot LanternSlot
        {
            get { return (SpellSlot) 62; }
        }

        public static SpellDataInst LanternSpell
        {
            get { return Player.Spellbook.GetSpell(LanternSlot); }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            if (!ThreshInGame())
            {
                return;
            }

            Menu = new Menu("AutoLantern", "AutoLantern", true);
            Menu.AddItem(new MenuItem("Auto", "Auto-Lantern at Low HP").SetValue(true));
            Menu.AddItem(new MenuItem("Low", "Low HP Percent").SetValue(new Slider(20, 10, 50)));
            Menu.AddItem(new MenuItem("Hotkey", "Hotkey").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Draw", "Draw Helper Text").SetValue(true));
            Menu.AddToMainMenu();

            LanternText = new Render.Text("Click Lantern", Drawing.Width/2 - Drawing.Width/3, Drawing.Height/2 + Drawing.Height/3, 28, Color.Red, "Verdana")
            {
                VisibleCondition = sender => Menu.Item("Draw").IsActive()
            };

            LanternText.Add();

            Game.OnUpdate += OnGameUpdate;
        }

        private static void OnGameUpdate(EventArgs args)
        {
            if (LanternSpell == null || !LanternSpell.Name.Equals("LanternWAlly"))
            {
                LanternText.Color = Color.Red;
                LanternText.text = "Click Lantern";
                return;
            }

            LanternText.Color = new ColorBGRA(0, 250, 0, 255);
            LanternText.text = "AutoLantern Ready";

            if (Menu.Item("Auto").IsActive() && IsLow() && UseLantern())
            {
                return;
            }

            if (!Menu.Item("Hotkey").IsActive())
            {
                return;
            }

            UseLantern();
        }

        private static bool UseLantern()
        {
            var lantern =
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(
                        o => o.IsValid && o.IsAlly && o.Name.Equals("ThreshLantern") && Player.Distance(o) <= 500);

            return lantern != null && Player.Spellbook.CastSpell(LanternSlot, lantern);
        }

        private static bool IsLow()
        {
            return Player.HealthPercent <= Menu.Item("Low").GetValue<Slider>().Value;
        }

        private static bool ThreshInGame()
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsAlly && !h.IsMe && h.ChampionName.Equals("Thresh"));
        }
    }
}