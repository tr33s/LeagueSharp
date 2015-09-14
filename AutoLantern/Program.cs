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
        public static SpellSlot LanternSlot = (SpellSlot) 62;
        public static int LastLantern;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
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

            LanternText = new Render.Text(
                "Click Lantern", Drawing.Width / 2 - Drawing.Width / 3, Drawing.Height / 2 + Drawing.Height / 3, 28,
                Color.Red, "Verdana") { VisibleCondition = sender => Menu.Item("Draw").IsActive() };

            LanternText.Add();

            Game.OnUpdate += OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender is Obj_AI_Hero && sender.IsAlly && args.SData.Name.Equals("LanternWAlly"))
            {
                LastLantern = Utils.TickCount;
            }
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

            return lantern != null && lantern.IsVisible && Utils.TickCount - LastLantern > 5000 &&
                   Player.Spellbook.CastSpell(LanternSlot, lantern);
        }

        private static bool IsLow()
        {
            return Player.HealthPercent <= Menu.Item("Low").GetValue<Slider>().Value;
        }

        private static bool ThreshInGame()
        {
            return HeroManager.Allies.Any(h => !h.IsMe && h.ChampionName.Equals("Thresh"));
        }
    }
}