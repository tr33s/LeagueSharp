#region

using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace TUrgot
{
    internal class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, Q2, W, E;
        public static SpellDataInst Ignite;
        public static Menu Menu;

        public static SpellDataInst R
        {
            get { return Player.Spellbook.GetSpell(SpellSlot.R); }
        }

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static Color ScriptColor
        {
            get { return Color.Cyan; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.IsChampion("Urgot"))
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1000);
            Q2 = new Spell(SpellSlot.Q, 1200);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 850);

            Q.SetSkillshot(0.2667f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.3f, 60f, 1800f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.2658f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            Menu = new Menu("TUrgot", "TreesUrgot", true);


            Orbwalker = Menu.AddOrbwalker();

            var q = Menu.AddMenu("Q", "Q Settings");

            q.AddBool("ComboQ", "Use in Combo");
            q.Item("ComboQ").SetSpellTooltip("Combo", "Q", ScriptColor);

            q.AddBool("HarassQ", "Use in Harass");
            q.Item("HarassQ").SetSpellTooltip("Harass", "Q", ScriptColor);

            q.AddBool("AutoQ", "Smart Q");
            q.Item("AutoQ")
                .SetTooltip(
                    "Automatically uses Q on nearby enemies with Urgot's E Buff.", System.Drawing.Color.Cyan.ToBGRA());

            q.AddBool("LaneClearQ", "Use in LaneClear");
            q.Item("LaneClearQ").SetSpellTooltip("LaneClear", "Q", ScriptColor);

            q.AddSlider("LaneClearQManaPercent", "Minimum Q Mana Percent", 30);
            q.Item("LaneClearQManaPercent").SetManaTooltip(ScriptColor, "Q");

            q.AddCircle("QRange", "Q", System.Drawing.Color.Red, Q.Range);
            q.Item("QRange").SetDrawingTooltip("Q", ScriptColor);

            var e = Menu.AddMenu("E", "E Settings");

            e.AddBool("ComboE", "Use in Combo");
            e.Item("ComboE").SetSpellTooltip("Combo", "E", ScriptColor);

            e.AddBool("HarassE", "Use in Harass");
            e.Item("HarassE").SetSpellTooltip("Harass", "E", ScriptColor);

            e.AddCircle("ERange", "E", System.Drawing.Color.Blue, E.Range);
            e.Item("ERange").SetDrawingTooltip("E", ScriptColor);

            Menu.AddBool("Interrupt", "Interrupt with R");
            Menu.Item("Interrupt").SetTooltip("Cast Urgot's R to interrupt enemy abilities.", ScriptColor);

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;

            Game.PrintChat("Trees Urgot loaded!");
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("Interrupt").GetValue<bool>() || !R.IsReady() ||
                !sender.IsValidTarget(400 + 150 * Player.Spellbook.GetSpell(SpellSlot.R).Level) ||
                args.DangerLevel < Interrupter2.DangerLevel.High)
            {
                return;
            }

            Player.Spellbook.CastSpell(SpellSlot.R, sender);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            if (Orbwalker.ActiveMode.IsFarmMode() && !IsManaLow())
            {
                LaneClear();
                return;
            }

            CastLogic();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Circle[] draw = { Menu.Item("QRange").GetValue<Circle>(), Menu.Item("ERange").GetValue<Circle>() };

            foreach (var circle in draw.Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static void LaneClear()
        {
            if (!Q.IsReady())
            {
                return;
            }

            var minions = MinionManager.GetMinions(Q.Range);
            var killable = minions.FirstOrDefault(m => Q.GetDamage(m) > m.Health);
            if (killable != null)
            {
                CastQ(killable);
                return;
            }
            var unit =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        minion =>
                            MinionManager.IsMinion(minion) &&
                            minion.IsValidTarget(minion.HasUrgotEBuff() ? Q2.Range : Q.Range) &&
                            minion.Health <= Q.GetDamage(minion));
            if (unit != null)
            {
                CastQ(unit, "LaneClear");
            }
        }

        private static void CastLogic()
        {
            if (SmartQ())
            {
                return;
            }

            var target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
            var mode = Orbwalker.ActiveMode.GetModeString();

            if (target == null || !Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            if (CastE(target, mode))
            {
                return;
            }

            CastQ(target, mode);
        }

        private static bool SmartQ()
        {
            if (!Q.IsReady() || !Menu.Item("AutoQ").IsActive())
            {
                return false;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(obj => obj.IsValidTarget(Q2.Range) && obj.HasUrgotEBuff())
                    .MinOrDefault(obj => obj.HealthPercent);

            if (unit != null && unit.IsValid)
            {
                W.Cast();
                return Q2.Cast(unit).IsCasted();
            }

            return false;
        }

        private static bool CastQ(Obj_AI_Base target, string mode = null)
        {
            if (!Q.IsReady() || (mode != null && !Menu.Item(mode + "Q").IsActive()))
            {
                return false;
            }

            if (target.HasUrgotEBuff() && target.IsValidTarget(Q2.Range) && Q2.Cast(target).IsCasted())
            {
                return true;
            }

            return target.IsValidTarget(Q.Range) && Q.Cast(target).IsCasted();
        }

        private static bool CastE(Obj_AI_Base target, string mode)
        {
            return E.IsReady() && Menu.Item(mode + "E").IsActive() && target.IsValidTarget(E.Range - 10) &&
                   E.Cast(target).IsCasted();
        }

        private static bool IsManaLow()
        {
            return Player.ManaPercent < Menu.Item("LaneClearQManaPercent").GetValue<Slider>().Value;
        }
    }
}