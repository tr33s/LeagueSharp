#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

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

            Ignite = Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot"));

            Menu = new Menu("TUrgot", "TreesUrgot", true);

            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            var ts = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);

            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboE", "Use E").SetValue(true));
            Menu.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassE", "Use E").SetValue(true));
            Menu.SubMenu("Harass")
                .AddItem(new MenuItem("HarassActive", "Harass").SetValue(new KeyBind((byte) 'C', KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
            Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearQ", "Use Q").SetValue(true));
            Menu.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearQManaPercent", "Minimum Q Mana Percent").SetValue(new Slider(30)));
            Menu.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "LaneClear").SetValue(new KeyBind((byte) 'V', KeyBindType.Press)));

            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q").SetValue(new Circle(false, Color.Red, Q.Range)));
            Menu.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E").SetValue(new Circle(false, Color.Blue, E.Range)));

            Menu.AddItem(new MenuItem("AutoQ", "Smart Q").SetValue(true));
            Menu.AddItem(new MenuItem("Interrupt", "Interrupt with Ult").SetValue(true));

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
            if (Player.IsDead || Player.IsDashing() || Player.Spellbook.IsAutoAttacking || Player.IsWindingUp)
            {
                return;
            }

            if (Menu.Item("LaneClearActive").IsActive() && !IsManaLow())
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

            var unit =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        minion =>
                            MinionManager.IsMinion(minion) &&
                            minion.IsValidTarget(minion.HasBuff("urgotcorrosivedebuff") ? Q2.Range : Q.Range) &&
                            minion.Health <= Q.GetDamage(minion));
            if (unit != null)
            {
                CastQ(unit, "LaneClear");
            }
        }

        private static void CastLogic()
        {
            SmartQ();

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || (!Menu.Item("ComboActive").IsActive() && !Menu.Item("HarassActive").IsActive()))
            {
                return;
            }

            var mode = Menu.Item("ComboActive").IsActive() ? "Combo" : "Harass";

            CastE(TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical), mode);
            CastQ(target, mode);
        }

        private static void SmartQ()
        {
            if (!Q.IsReady() || !Menu.Item("AutoQ").IsActive())
            {
                return;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(obj => obj.IsValidTarget(Q2.Range) && obj.HasBuff("urgotcorrosivedebuff"));

            if (unit != null && unit.IsValid)
            {
                W.Cast();
                Q2.Cast(unit);
            }
        }

        private static void CastQ(Obj_AI_Base target, string mode)
        {
            if (Q.IsReady() && Menu.Item(mode + "Q").IsActive() && target.IsValidTarget(Q.Range))
            {
                Q.Cast(target);
            }
        }

        private static void CastE(Obj_AI_Base target, string mode)
        {
            if (!E.IsReady() || !Menu.Item(mode + "E").IsActive() || !target.IsValidTarget(E.Range))
            {
                return;
            }

            E.Cast(target);
        }

        private static bool IsManaLow()
        {
            return Player.ManaPercent < Menu.Item("LaneClearQManaPercent").GetValue<Slider>().Value;
        }
    }
}