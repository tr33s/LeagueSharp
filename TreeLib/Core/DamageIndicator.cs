using System;
using System.Linq;
using System.Security.Permissions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Extensions;

namespace TreeLib.Core
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public static class DamageIndicator
    {
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;
        private static DamageToUnitDelegate DamageToUnit;
        private static readonly Render.Text Text = new Render.Text(0, 0, "", 16, Color.DeepPink);
        private static readonly Render.Rectangle DamageBar = new Render.Rectangle(0, 0, 1, 8, Color.White);
        private static readonly Render.Line HealthLine = new Render.Line(Vector2.Zero, Vector2.Zero, 1, Color.White);
        private static Menu Menu;

        public static bool PredictedHealth
        {
            get { return Menu.Item("HPColor").IsActive(); }
        }

        public static bool Fill
        {
            get { return Menu.Item("FillColor").IsActive(); }
        }

        public static System.Drawing.Color HealthColor
        {
            get { return Menu.Item("HPColor").GetValue<Circle>().Color; }
        }

        public static bool Killable
        {
            get { return Menu.Item("Killable").IsActive(); }
        }

        public static System.Drawing.Color DamageColor
        {
            get { return Menu.Item("FillColor").GetValue<Circle>().Color; }
        }

        public static bool Enabled
        {
            get { return Menu.Item("DmgEnabled").IsActive(); }
        }

        public static void Initialize(Menu menu, DamageToUnitDelegate comboDmg)
        {
            Menu = menu;
            DamageToUnit = comboDmg;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

            foreach (
                var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsHPBarRendered && h.IsEnemy))
            {
                var barPos = unit.HPBarPosition;
                var damage = DamageToUnit(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;

                var champOffset = unit.ChampionName == "Jhin" ? new Vector2(-8, -14) : Vector2.Zero;
                var xPos = barPos.X + XOffset + champOffset.X;
                var xPosDamage = xPos + Width * percentHealthAfterDamage;
                var xPosCurrentHp = xPos + Width * unit.Health / unit.MaxHealth;
                var yPos = barPos.Y + YOffset + champOffset.Y;

                if (Killable && damage > unit.Health)
                {
                    Text.X = (int) barPos.X + XOffset;
                    Text.Y = (int) barPos.Y + YOffset + 20;
                    Text.text = "KILLABLE";
                    Text.OnEndScene();
                }

                if (Fill)
                {
                    var differenceInHp = xPosCurrentHp - xPosDamage;
                    DamageBar.Color = DamageColor.ToSharpDXColor();
                    DamageBar.X = (int) (barPos.X + 9 + 107 * percentHealthAfterDamage);
                    DamageBar.Y = (int) yPos - 1;
                    DamageBar.Width = (int) Math.Round(differenceInHp);
                    DamageBar.Height = Height + 3;
                    DamageBar.OnEndScene();
                }

                if (PredictedHealth)
                {
                    HealthLine.Start = new Vector2(xPosDamage, yPos - 1);
                    HealthLine.End = new Vector2(xPosDamage, yPos + Height);
                    HealthLine.Width = 2;
                    HealthLine.Color = HealthColor.ToSharpDXColor();
                    HealthLine.OnEndScene();
                }
            }
        }
    }
}