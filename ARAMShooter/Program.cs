using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace ARAMShooter
{
    internal class Program
    {
        public static Spell Throw;
        public static Menu Menu;

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            var spell = ObjectManager.Player.GetSpellSlot("summonersnowball");

            if (spell == SpellSlot.Unknown || !Game.MapId.Equals(GameMapId.HowlingAbyss))
            {
                return;
            }

            Menu = new Menu("ARAMShooter", "ARAMShooter", true);
            Menu.AddItem(new MenuItem("DecreaseRange", "Decrease Range by").SetValue(new Slider(10)));
            Menu.AddItem(
                new MenuItem("HitChance", "MinHitChance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString() }, 1)));
            Menu.AddItem(new MenuItem("Auto", "AutoDash").SetValue(true));
            Menu.AddItem(new MenuItem("Throw", "Throw Key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Menu.AddItem(new MenuItem("Stealth", "Throw When Stealthed").SetValue(false));

            Menu.Item("HitChance").ValueChanged += Program_ValueChanged;
            Menu.Item("DecreaseRange").ValueChanged += Program_ValueChanged1;
            Menu.AddToMainMenu();

            Throw = new Spell(spell, 1200f);
            Throw.SetSkillshot(.33f, 50f, 1600, true, SkillshotType.SkillshotLine);
            Throw.MinHitChance = GetHitChance();

            ShowNotification("ARAMShooter - Loaded", Color.FromArgb(0, 250, 255), 3000);

            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Throw.IsReady())
            {
                return;
            }

            if (Throw.Instance.Name.Equals("snowballfollowupcast"))
            {
                if (!Menu.Item("Auto").IsActive())
                {
                    return;
                }

                Utility.DelayAction.Add(600, () => Throw.Cast());
                return;
            }

            if (!Menu.Item("Throw").IsActive() ||
                (ObjectManager.Player.HasBuffOfType(BuffType.Invisibility) && !Menu.Item("Stealth").IsActive()))
            {
                return;
            }

            foreach (var champ in
                ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(Throw.Range)))
            {
                Throw.Cast(champ);
            }
        }

        private static void Program_ValueChanged1(object sender, OnValueChangeEventArgs e)
        {
            Throw.Range = 1200f - e.GetNewValue<Slider>().Value;
        }

        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            Throw.MinHitChance = GetHitChance();
        }

        private static HitChance GetHitChance()
        {
            var hc = Menu.Item("HitChance").GetValue<StringList>();
            switch (hc.SList[hc.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
            }
            return HitChance.Medium;
        }

        public static void ShowNotification(string message, Color color, int duration = -1, bool dispose = true)
        {
            Notifications.AddNotification(new Notification(message, duration, dispose).SetTextColor(color));
        }
    }
}