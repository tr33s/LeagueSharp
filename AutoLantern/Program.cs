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
            Menu.AddItem(new MenuItem("LanternReady", "Lantern Ready").SetValue(false));
            Menu.AddItem(new MenuItem("PermaShowLantern", "PermaShow Helper").SetValue(true));

            if (Menu.Item("PermaShowLantern").IsActive())
            {
                var active = IsLanternSpellActive();
                Menu.Item("LanternReady")
                    .Permashow(
                        true, active ? "[AutoLantern] Lantern Ready" : "[AutoLantern] Click Lantern",
                        active ? Color.Green : Color.Red);
            }

            Menu.Item("PermaShowLantern").ValueChanged += (sender, eventArgs) =>
            {
                var lanternActive = IsLanternSpellActive();
                Menu.Item("LanternReady")
                    .Permashow(
                        eventArgs.GetNewValue<bool>(),
                        lanternActive ? "[AutoLantern] Lantern Ready" : "[AutoLantern] Click Lantern",
                        lanternActive ? Color.Green : Color.Red);
            };

            Menu.AddToMainMenu();

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
            if (!IsLanternSpellActive())
            {
                Menu.Item("LanternReady").SetValue(false);
                return;
            }

            Menu.Item("LanternReady").SetValue(true);

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

        private static bool IsLanternSpellActive()
        {
            return LanternSpell != null && LanternSpell.Name.Equals("LanternWAlly");
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