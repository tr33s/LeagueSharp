using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Babehri
{
    internal class Program
    {
        public static MissileClient AhriQ;
        public static Vector3 StartPosition;
        public static Vector3 EndPosition;
        public static AttackableUnit LastAutoTarget;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static string Mode;
        public static Geometry.Polygon.Line Line;
        public static float StartTime;
        public static List<Vector2Time> Path;
        public static Render.Text PassiveText = new Render.Text("", 0, 0, 24, Color.White, "Tahoma");

        public static GameObject QObject
        {
            get
            {
                return AhriQ ??
                       ObjectManager.Get<GameObject>()
                           .FirstOrDefault(
                               obj =>
                                   obj.IsValid &&
                                   (obj.Name.Contains("Ahri_Base_Orb") || obj.Name.Contains("Ahri_Orb") ||
                                    obj.Name.Contains("Ahri_Passive")));
            }
        }

        public static int PassiveStack
        {
            get
            {
                var buff = Player.Buffs.FirstOrDefault(b => b.Name.Equals("ahrisoulcrushercounter"));
                return buff == null ? 0 : buff.Count;
            }
        }

        public static int UltStack
        {
            get
            {
                var buff = Player.Buffs.FirstOrDefault(b => b.Name.Equals("AhriTumble"));
                return buff == null ? 3 : buff.Count;
            }
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
            if (!Player.IsChampion("Ahri"))
            {
                return;
            }

            Menu = new Menu("Babehri", "Babehri", true);

            Orbwalker = Menu.AddOrbwalker();
            Menu.AddTargetSelector();

            var combo = Menu.AddMenu("Combo", "Combo");
            combo.AddBool("ComboQ", "Use Q");
            combo.AddBool("ComboW", "Use W");
            combo.AddSlider("ComboWMinHit", "Min Fox-Fire Hits", 2, 0, 3);
            combo.AddBool("ComboE", "Use E");
            //combo.AddItem(new MenuItem("ComboItems", "Use Items").SetValue(true));

            var harass = Menu.AddMenu("Harass", "Harass");
            harass.AddBool("HarassQ", "Use Q");
            harass.AddBool("HarassW", "Use W");
            harass.AddSlider("HarassWMinHit", "Min Fox-Fire Hits", 2, 0, 3);
            harass.AddBool("HarassE", "Use E");
            harass.AddSlider("HarassMinMana", "Min Mana Percent", 30);

            var farm = Menu.AddMenu("Farm", "Farm");
            farm.AddBool("Farm-Use-Q", "Use Smart Q for LH");
            farm.AddBool("Farm-Use-Q-LH", "Use Q only for LH");
            farm.AddSlider("Farm-Mana", "Minimum Mana %", 50);

            var misc = Menu.AddMenu("Misc", "Misc");
            var eMisc = misc.AddMenu("E", "E");
            eMisc.AddBool("GapcloseE", "Use E on Gapclose");
            eMisc.AddBool("InterruptE", "Use E to Interrupt");
            var rMisc = misc.AddMenu("R", "R");
            rMisc.AddSlider("DamageR", "R Dmg Prediction Bolt Count", 2, 0, 3);
            rMisc.AddKeyBind("FleeR", "Use R Flee", 'T');

            var drawing = Menu.AddMenu("Drawing", "Drawing");

            var damage = drawing.AddMenu("Damage Indicator", "Damage Indicator");
            damage.AddBool("DmgEnabled", "Enabled");
            damage.AddCircle("HPColor", "Health Color", System.Drawing.Color.White);
            damage.AddCircle("FillColor", "Damage Color", System.Drawing.Color.DeepPink);

            drawing.AddCircle("DrawQ", "Draw Q", System.Drawing.Color.DeepPink, Spells.Q.Range);
            drawing.AddCircle("DrawW", "Draw W", System.Drawing.Color.White, Spells.W.Range);
            drawing.AddCircle("DrawE", "Draw E", System.Drawing.Color.MediumVioletRed, Spells.E.Range);
            drawing.AddCircle("DrawR", "Draw R", System.Drawing.Color.Cyan, Spells.R.Range);
            drawing.AddCircle("DrawPassive", "Draw Passive Stack", System.Drawing.Color.White);

            Menu.AddToMainMenu();

            PassiveText.VisibleCondition += sender => Menu.Item("DrawPassive").IsActive();
            PassiveText.Add();

            DamageIndicator.DamageToUnit = GetComboDamage;

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("GapcloseE").IsActive() && Player.Distance(gapcloser.End) < 100 && Spells.E.IsReady())
            {
                Spells.E.Cast(gapcloser.Sender);
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (Menu.Item("InterruptE").IsActive() && Spells.E.CanCast(sender))
            {
                Spells.E.Cast(sender);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;
            var target = args.Target as AttackableUnit;

            if (unit == null || !unit.IsValid || !unit.IsMe || target == null || !target.IsValid ||
                !args.SData.IsAutoAttack())
            {
                return;
            }

            LastAutoTarget = target;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            Flee();
            HandlePassive();

            var activeMode = Orbwalker.ActiveMode;
            var mode = activeMode.GetModeString();
            var target = TargetSelector.GetTarget(Spells.E.Range, TargetSelector.DamageType.Magical);

            if (activeMode.IsComboMode())
            {
                if (target == null || !target.IsValidTarget(Spells.E.Range))
                {
                    return;
                }

                if (mode.Equals("Harass") && Player.ManaPercent < Menu.Item("HarassMinMana").GetValue<Slider>().Value)
                {
                    return;
                }

                if (Spells.E.IsActive() && CastE(target))
                {
                    return;
                }

                if (Spells.Q.IsActive() && CastQ(target))
                {
                    return;
                }

                if (Spells.W.IsActive() && CastW(target))
                {
                    return;
                }
            }

            if (Spells.Q.IsReady() &&
                (Player.ManaPercent >= Menu.Item("Farm-Mana").GetValue<Slider>().Value &&
                 (activeMode == Orbwalking.OrbwalkingMode.LaneClear || activeMode == Orbwalking.OrbwalkingMode.Mixed) &&
                 (Menu.Item("Farm-Use-Q-LH").IsActive() ||
                  (!Menu.Item("Farm-Use-Q-LH").IsActive() && !ShouldWaitForMinionKill()))))
            {
                Farm();
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;
            if (missile == null || !missile.IsValid || !missile.SpellCaster.IsMe)
            {
                return;
            }

            if (missile.SData.Name.Equals("AhriOrbReturn") || missile.SData.Name.Equals("AhriOrbMissile"))
            {
                AhriQ = missile;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (AhriQ != null && sender.NetworkId.Equals(AhriQ.NetworkId))
            {
                AhriQ = null;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var circle in
                new[] { "Q", "W", "E", "R" }.Select(spell => Menu.Item("Draw" + spell).GetValue<Circle>())
                    .Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static bool CastQ(Obj_AI_Base target)
        {
            return Spells.Q.CanCast(target) && !Player.IsDashing() && Spells.Q.Cast(target).IsCasted();
        }

        private static bool CastW(AttackableUnit target)
        {
            var count = CountWHits(target);
            var hitCount = Menu.Item(Orbwalker.ActiveMode.GetModeString() + "WMinHit").GetValue<Slider>().Value;
            return Spells.W.IsReady() && target.IsValidTarget(Spells.W.Range) && count >= hitCount && Spells.W.Cast();
        }

        private static bool CastE(Obj_AI_Base target)
        {
            return Spells.E.CanCast(target) && Spells.E.Cast(target).IsCasted();
        }

        public static int CountWHits(AttackableUnit target)
        {
            return GetWTargets().Count(obj => obj.IsValid && obj.NetworkId.Equals(target.NetworkId));
        }

        public static List<AttackableUnit> GetWTargets()
        {
            var list = new List<AttackableUnit>();
            var range = Spells.W.Range;

            if (QObject != null)
            {
                var firstPriority =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.IsValidTarget(range))
                        .MinOrDefault(h => h.Distance(QObject.Position));

                if (firstPriority != null && firstPriority.IsValidTarget(range))
                {
                    list.Add(firstPriority);
                }

                var thirdPriority =
                    ObjectManager.Get<Obj_AI_Base>()
                        .Where(h => h.IsValid && h.IsEnemy && h.IsValidTarget(range))
                        .MinOrDefault(h => h.Distance(QObject.Position));

                if (thirdPriority != null && thirdPriority.IsValidTarget(range))
                {
                    list.Add(thirdPriority);
                }
            }

            if (LastAutoTarget != null && LastAutoTarget.IsValidTarget(range))
            {
                list.Add(LastAutoTarget);
            }

            return list;
        }

        public static void Flee()
        {
            if (!Menu.Item("FleeR").IsActive() || !Spells.R.IsReady())
            {
                return;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;
            var pos = Player.ServerPosition.Extend(Game.CursorPos, Spells.R.Range + 10);
            Spells.R.Cast(pos);
        }

        public static void HandlePassive()
        {
            var position = Drawing.WorldToScreen(QObject.Position);

            PassiveText.Visible = true;
            PassiveText.X = (int) position.X + 10;
            PassiveText.Y = (int) position.Y + 20;

            if (QObject == null || QObject.Position.Equals(Vector3.Zero))
            {
                PassiveText.Visible = false;
                return;
            }

            PassiveText.Color = Menu.Item("DrawPassive").GetValue<Circle>().Color.ToBGRA();

            if (QObject.Name.Contains("Passive"))
            {
                if (Menu.Item("DrawPassive").IsActive())
                {
                    Render.Circle.DrawCircle(QObject.Position, 35, System.Drawing.Color.LimeGreen, 7);
                }


                PassiveText.text = GetPassiveHeal().ToString();
                PassiveText.Color = Color.LimeGreen;
                return;
            }

            PassiveText.text = (9 - PassiveStack).ToString();
        }

        public static int GetPassiveHeal()
        {
            return (int) ((2 + Player.Level) * .9 * Player.TotalMagicalDamage);
        }

        public static float GetComboDamage(Obj_AI_Base unit)
        {
            var d = 0d;

            if (Spells.Q.IsReady())
            {
                d += Player.GetDamageSpell(unit, SpellSlot.Q).CalculatedDamage;
                d += Player.GetDamageSpell(unit, SpellSlot.Q, 1).CalculatedDamage;
            }

            if (Spells.W.IsReady())
            {
                var count = CountWHits(unit);
                if (count > 0)
                {
                    count--;
                    d += Player.GetDamageSpell(unit, SpellSlot.W).CalculatedDamage;
                    d += count * Player.GetDamageSpell(unit, SpellSlot.W, 1).CalculatedDamage;
                }
            }

            if (Spells.E.IsReady())
            {
                d += Player.GetDamageSpell(unit, SpellSlot.E).CalculatedDamage;
            }

            if (Spells.R.IsReady())
            {
                var stacks = Math.Min(UltStack, Menu.Item("DamageR").GetValue<Slider>().Value);
                d += stacks * Player.GetDamageSpell(unit, SpellSlot.R).CalculatedDamage;
            }
            return (float) d;
        }

        #region "General Farm Methods and Functions"

        private static bool ShouldWaitForMinionKill()
        {
            return
                ObjectManager.Get<Obj_AI_Minion>()
                    .Any(
                        minion =>
                            minion.IsValidTarget(Spells.Q.Range) && minion.Team != GameObjectTeam.Neutral &&
                            HealthPrediction.LaneClearHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * 2.2f), 0) < Spells.Q.GetDamage(minion));
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!Menu.Item("Farm-Use-Q").GetValue<bool>() &&
                (Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LaneClear) ||
                 Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LastHit)) || !Spells.Q.IsReady())
            {
                return;
            }

            if (
                MinionManager.GetMinions(Spells.Q.Range)
                    .Where(
                        minion =>
                            !target.NetworkId.Equals(minion.NetworkId) &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * 2.65f + Game.Ping / 2f), 0) <= 0 &&
                            Spells.Q.GetDamage(minion) >= minion.Health)
                    .Any(minionGoingToDie => Spells.Q.Cast(minionGoingToDie).IsCasted())) {}
        }

        private static void Farm()
        {
            var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical, false);

            //If we can hit the target and 2 other things, then cast Q
            if (Spells.Q.CastIfWillHit(target, 3))
            {
                return;
            }

            //  Otherwise, cast Q on the minion wave
            var minions = MinionManager.GetMinions(Player.ServerPosition, Spells.Q.Range);
            var qHit = Spells.Q.GetLineFarmLocation(minions);
            if (qHit.MinionsHit >= 4)
            {
                Spells.Q.Cast(qHit.Position);
            }
        }

        #endregion
    }
}