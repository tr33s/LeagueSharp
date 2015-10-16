using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace KindredSpirits
{
    internal static class PassiveManager
    {
        private const float WolfAutoAttackRange = 200f;
        private static Obj_AI_Minion Wolf;
        private static int WolfSpawnTime;
        private static Vector3 WolfSpawnLocation;
        private static Obj_AI_Base Lamb;
        private static Render.Text TimeText;
        private static Render.Text HealText;

        private static Menu Menu
        {
            get { return Program.Menu.SubMenu("Drawing"); }
        }

        public static void Initialize()
        {
            Menu.AddCircle("DrawWolfAARange", "Draw Wolf AA Range", Color.Purple);
            Menu.AddBool("DrawWolfTime", "Draw Wolf Time");
            Menu.AddBool("DrawWStacks", "Draw W Passive (Max Stacks)");

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;

            HealText = new Render.Text("", 0, 0, 20, SharpDX.Color.Green);
            HealText.VisibleCondition += sender => Menu.Item("DrawWStacks").IsActive() && IsWPassiveReady();
            HealText.PositionUpdate += () =>
            {
                var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
                if (Wolf != null && Wolf.IsValid && Wolf.IsVisible)
                {
                    pos = Drawing.WorldToScreen(Wolf.Position);
                }
                return pos + new Vector2(-35, 20);
            };
            HealText.TextUpdate += () => "Heal Ready (" + GetWPassiveHeal() + ")";
            HealText.TextFontDescription = new FontDescription
            {
                FaceName = "Calibri",
                Height = 20,
                OutputPrecision = FontPrecision.TrueType,
                Quality = FontQuality.Antialiased
            };
            HealText.Add();

            TimeText = new Render.Text("", 0, 0, 40, Color.Purple.ToBGRA());
            TimeText.VisibleCondition +=
                sender => Menu.Item("DrawWolfTime").IsActive() && Wolf != null && Wolf.IsValid && Wolf.IsVisible;
            TimeText.PositionUpdate += () => Wolf.HPBarPosition + new Vector2(30, -10);
            TimeText.TextUpdate += () =>
            {
                var timeSpawned = Utils.TickCount - WolfSpawnTime;
                var timeLeft = Math.Round((8000 - timeSpawned) / 1000f, 1, MidpointRounding.ToEven);
                if (timeLeft < 0)
                {
                    return "";
                }
                var time = timeLeft.ToString();
                return time.Substring(0, time.Length > 3 ? 3 : time.Length);
            };
            TimeText.TextFontDescription = new FontDescription
            {
                FaceName = "Calibri",
                Height = 40,
                OutputPrecision = FontPrecision.TrueType,
                Quality = FontQuality.Antialiased
            };
            TimeText.Add();
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Minion;
            if (unit != null && unit.IsValid && unit.IsAlly && unit.CharData.BaseSkinName.Equals("kindredwolf"))
            {
                Wolf = unit;
                WolfSpawnTime = Utils.TickCount;
                WolfSpawnLocation = unit.Position;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (Wolf != null && sender.NetworkId.Equals(Wolf.NetworkId))
            {
                Wolf = null;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("DrawWolfAARange").IsActive() || Wolf == null || !Wolf.IsValid || !Wolf.IsVisible)
            {
                return;
            }

            Render.Circle.DrawCircle(
                Wolf.Position, WolfAutoAttackRange, Menu.Item("DrawWolfAARange").GetValue<Circle>().Color);
            Render.Circle.DrawCircle(WolfSpawnLocation, 850, Color.Black);
        }

        public static bool IsInWolfRange(this Obj_AI_Base unit)
        {
            return Wolf != null && Wolf.IsValid && Wolf.IsVisible && WolfSpawnLocation.Distance(unit.Position) < 850;
        }

        public static int GetWPassiveCount()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name.Equals("kindredwchargepassivebuff"));
            if (buff == null || !buff.IsValid || !buff.IsActive)
            {
                return 0;
            }

            return buff.Count;
        }

        public static bool IsWPassiveReady()
        {
            return GetWPassiveCount() == 100;
        }

        public static int GetWPassiveHeal()
        {
            return 60 + 3 * ObjectManager.Player.Level;
        }
    }
}