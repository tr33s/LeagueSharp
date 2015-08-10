using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace jesuisFiora
{
    internal class DamageIndicator
    {
        public delegate float DamageToUnitDelegate(Obj_AI_Hero hero);

        private const int XOffset = 10;
        private const int YOffset = 20;
        private const int Width = 103;
        private const int Height = 8;
        private static DamageToUnitDelegate _damageToUnit;

        private static readonly Render.Text Text = new Render.Text(
            0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");

        public static bool PredictedHealth
        {
            get { return Program.Menu.Item("HPColor").GetValue<Circle>().Active; }
        }

        public static bool Fill
        {
            get { return Program.Menu.Item("FillColor").GetValue<Circle>().Active; }
        }

        public static Color Color
        {
            get { return Program.Menu.Item("HPColor").GetValue<Circle>().Color; }
        }

        public static Color FillColor
        {
            get { return Program.Menu.Item("FillColor").GetValue<Circle>().Color; }
        }

        public static bool Enabled
        {
            get { return Program.Menu.Item("DamageIndicator").GetValue<bool>(); }
        }

        public static DamageToUnitDelegate DamageToUnit
        {
            get { return _damageToUnit; }

            set
            {
                if (_damageToUnit == null)
                {
                    Drawing.OnDraw += Drawing_OnDraw;
                }
                _damageToUnit = value;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Enabled || _damageToUnit == null)
            {
                return;
            }

            foreach (
                var unit in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && h.IsEnemy && h.IsHPBarRendered))
            {
                var barPos = unit.HPBarPosition;
                var damage = _damageToUnit(unit);
                var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                var yPos = barPos.Y + YOffset;
                var xPosDamage = barPos.X + XOffset + Width * percentHealthAfterDamage;
                var xPosCurrentHp = barPos.X + XOffset + Width * unit.Health / unit.MaxHealth;

                if (damage > unit.Health)
                {
                    Text.X = (int) barPos.X + XOffset;
                    Text.Y = (int) barPos.Y + YOffset - 13;
                    Text.text = ((int) (unit.Health - damage)).ToString();
                    Text.OnEndScene();
                }

                if (PredictedHealth)
                {
                    Drawing.DrawLine(xPosDamage, yPos, xPosDamage, yPos + Height, 4, Color);
                }

                if (Fill)
                {
                    var differenceInHP = xPosCurrentHp - xPosDamage;
                    var pos1 = barPos.X + 9 + (107 * percentHealthAfterDamage);

                    for (var i = 0; i < differenceInHP; i++)
                    {
                        Drawing.DrawLine(pos1 + i, yPos, pos1 + i, yPos + Height, 1, FillColor);
                    }
                }
            }
        }
    }
}