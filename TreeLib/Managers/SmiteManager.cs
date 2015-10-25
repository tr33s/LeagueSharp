using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Extensions;
using Color = System.Drawing.Color;

namespace TreeLib.Managers
{
    internal static class SmiteManager
    {
        private static Obj_AI_Base Minion;

        private static readonly string[] SmiteableMinions =
        {
            "SRU_Red", "SRU_Blue", "SRU_Dragon", "SRU_Baron",
            "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Krug", "Sru_Crab", "TT_Spiderboss", "TTNGolem", "TTNWolf",
            "TTNWraith"
        };

        private static Menu Menu;

        private static Spell Smite
        {
            get { return SpellManager.Smite; }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static IEnumerable<Obj_AI_Base> NearbyMinions
        {
            get
            {
                return MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral);
            }
        }

        public static void Initialize()
        {
            Menu = SpellManager.Menu.AddMenu("Smite", "Smite");
            Menu.AddKeyBind("Enabled", "Smite Enabled", 'K', KeyBindType.Toggle, true);
            Menu.AddBool("DrawSmite", "Draw Smite Range");
            Menu.AddBool("DrawDamage", "Draw Smite Damage");
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Minion != null && !Minion.IsValidTarget(float.MaxValue, false))
            {
                Minion = null;
            }

            if (!Menu.Item("Enabled").IsActive() || Player.IsDead || !Smite.IsReady())
            {
                return;
            }

            var minion =
                NearbyMinions.FirstOrDefault(
                    buff => buff.IsValidTarget() && SmiteableMinions.Contains(buff.CharData.BaseSkinName));

            if (minion == null || !minion.IsValid)
            {
                return;
            }

            Minion = minion;

            if (Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) > Minion.Health)
            {
                Smite.CastOnUnit(Minion);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || !Menu.Item("Enabled").IsActive())
            {
                return;
            }

            if (Menu.Item("DrawSmite").IsActive())
            {
                if (!Smite.IsReady())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Red);

                    return;
                }
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 500, Color.Green);
            }

            if (Menu.Item("DrawDamage").IsActive() && Minion != null && Minion.IsValid && !Minion.IsDead &&
                Minion.IsVisible && Minion.IsHPBarRendered)
            {
                DrawMinion(Minion);
            }
        }

        private static void DrawMinion(Obj_AI_Base minion)
        {
            var hpBarPosition = minion.HPBarPosition;
            var maxHealth = minion.MaxHealth;
            var sDamage = Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite);
            var x = sDamage / maxHealth;
            var barWidth = 0;

            switch (minion.CharData.BaseSkinName)
            {
                case "SRU_Red":
                case "SRU_Blue":
                case "SRU_Dragon":
                    barWidth = 145;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X + 3 + (float) (barWidth * x), hpBarPosition.Y + 18),
                        new Vector2(hpBarPosition.X + 3 + (float) (barWidth * x), hpBarPosition.Y + 28), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X - 22 + (float) (barWidth * x), hpBarPosition.Y, Color.Chartreuse,
                        sDamage.ToString());
                    break;
                case "SRU_Baron":
                    barWidth = 194;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X - 22 + (float) (barWidth * x), hpBarPosition.Y + 13),
                        new Vector2(hpBarPosition.X - 22 + (float) (barWidth * x), hpBarPosition.Y + 29), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X - 22 + (float) (barWidth * x), hpBarPosition.Y - 3, Color.Chartreuse,
                        sDamage.ToString());
                    break;
                case "Sru_Crab":
                    barWidth = 61;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X + 45 + (float) (barWidth * x), hpBarPosition.Y + 34),
                        new Vector2(hpBarPosition.X + 45 + (float) (barWidth * x), hpBarPosition.Y + 37), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X + 40 + (float) (barWidth * x), hpBarPosition.Y + 16, Color.Chartreuse,
                        sDamage.ToString());
                    break;
                case "SRU_Murkwolf":
                    barWidth = 75;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X + 54 + (float) (barWidth * x), hpBarPosition.Y + 19),
                        new Vector2(hpBarPosition.X + 54 + (float) (barWidth * x), hpBarPosition.Y + 23), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X + 50 + (float) (barWidth * x), hpBarPosition.Y, Color.Chartreuse,
                        sDamage.ToString());
                    break;
                case "SRU_Razorbeak":
                    barWidth = 75;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X + 54 + (float) (barWidth * x), hpBarPosition.Y + 18),
                        new Vector2(hpBarPosition.X + 54 + (float) (barWidth * x), hpBarPosition.Y + 22), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X + 54 + (float) (barWidth * x), hpBarPosition.Y, Color.Chartreuse,
                        sDamage.ToString());
                    break;
                case "SRU_Krug":
                    barWidth = 81;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X + 58 + (float) (barWidth * x), hpBarPosition.Y + 18),
                        new Vector2(hpBarPosition.X + 58 + (float) (barWidth * x), hpBarPosition.Y + 22), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X + 54 + (float) (barWidth * x), hpBarPosition.Y, Color.Chartreuse,
                        sDamage.ToString());
                    break;
                case "SRU_Gromp":
                    barWidth = 87;
                    Drawing.DrawLine(
                        new Vector2(hpBarPosition.X + 62 + (float) (barWidth * x), hpBarPosition.Y + 18),
                        new Vector2(hpBarPosition.X + 62 + (float) (barWidth * x), hpBarPosition.Y + 22), 2f,
                        Color.Chartreuse);
                    Drawing.DrawText(
                        hpBarPosition.X + 58 + (float) (barWidth * x), hpBarPosition.Y, Color.Chartreuse,
                        sDamage.ToString());
                    break;
            }
        }
    }
}