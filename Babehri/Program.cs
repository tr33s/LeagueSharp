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
        public static MissileClient AhriQMissile;
        public static Obj_GeneralParticleEmitter AhriQParticle;
        public static AttackableUnit LastAutoTarget;
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Render.Text PassiveText = new Render.Text("", 0, 0, 24, Color.White, "Tahoma");

        public static GameObject QObject
        {
            get
            {
                if (AhriQMissile != null && AhriQMissile.IsValid)
                {
                    return AhriQMissile;
                }

                if (AhriQParticle != null && AhriQParticle.IsValid)
                {
                    return AhriQParticle;
                }
                return null;
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
            farm.AddBool("FarmQ", "Smart Farm with Q");
            farm.AddSlider("FarmQHC", "Q Min HitCount", 3, 1, 5);
            farm.AddBool("FarmQLH", "Save Q for LH", false);
            farm.AddBool("FarmW", "Use W", false);
            farm.AddSlider("FarmMana", "Minimum Mana %", 50);

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
            drawing.AddCircle("DrawW", "Draw W", System.Drawing.Color.White, Spells.W.Range, false);
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
            if (Player.IsDead || Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            Flee();
            Farm();

            var activeMode = Orbwalker.ActiveMode;
            var mode = activeMode.GetModeString();
            var target = TargetSelector.GetTarget(Spells.E.Range, TargetSelector.DamageType.Magical);

            if (!activeMode.IsComboMode())
            {
                return;
            }

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

            if (Spells.W.IsActive() && CastW(target)) {}
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var particle = sender as Obj_GeneralParticleEmitter;
            if (particle != null && particle.IsValid &&
                (particle.Name.Contains("Ahri_Orb") || particle.Name.Contains("Ahri_Passive")))
            {
                AhriQParticle = particle;
                return;
            }

            var missile = sender as MissileClient;
            if (missile == null || !missile.IsValid || !missile.SpellCaster.IsMe)
            {
                return;
            }

            if (missile.SData.Name.Equals("AhriOrbReturn") || missile.SData.Name.Equals("AhriOrbMissile"))
            {
                AhriQMissile = missile;
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (AhriQMissile != null && sender.NetworkId.Equals(AhriQMissile.NetworkId))
            {
                AhriQMissile = null;
            }

            if (AhriQParticle != null && sender.NetworkId.Equals(AhriQParticle.NetworkId))
            {
                AhriQParticle = null;
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            HandlePassive();

            foreach (var circle in
                new[] { "Q", "W", "E", "R" }.Select(spell => Menu.Item("Draw" + spell).GetValue<Circle>())
                    .Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static bool CastQ(Obj_AI_Base target)
        {
            if (!Spells.Q.IsReady() || Player.IsDashing() || !target.IsValidTarget(Spells.Q.Range))
            {
                return false;
            }

            for (var i = 5; i >= 2; i--)
            {
                if (Spells.Q.CastIfWillHit(target, i))
                {
                    return true;
                }
            }

            return Spells.Q.Cast(target).IsCasted();
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
                var objects = ObjectManager.Get<AttackableUnit>().Where(obj => obj.IsValidTarget(range));
                var attackableUnits = objects as AttackableUnit[] ?? objects.ToArray();

                var firstPriority =
                    attackableUnits.Where(obj => obj is Obj_AI_Hero)
                        .MinOrDefault(h => h.Position.Distance(QObject.Position));

                if (firstPriority != null && firstPriority.IsValidTarget(range))
                {
                    list.Add(firstPriority);
                }

                var thirdPriority = attackableUnits.MinOrDefault(h => h.Position.Distance(QObject.Position));

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
            if (!Menu.Item("FleeR").IsActive())
            {
                return;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (!Player.IsDashing() && Player.GetWaypoints().Last().Distance(Game.CursorPos) > 100)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (Spells.R.IsReady())
            {
                var pos = Player.ServerPosition.Extend(Game.CursorPos, Spells.R.Range + 10);
                Spells.R.Cast(pos);
            }
        }

        public static void HandlePassive()
        {
            if (QObject == null || QObject.Position.Equals(Vector3.Zero) || !QObject.IsVisible)
            {
                PassiveText.Visible = false;
                return;
            }

            var position = Drawing.WorldToScreen(QObject.Position);

            PassiveText.Visible = true;
            PassiveText.X = (int) position.X + 10;
            PassiveText.Y = (int) position.Y + 20;

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
            if (!Spells.Q.IsReady() || !Menu.Item("FarmQ").GetValue<bool>())
            {
                return;
            }


            if (!Orbwalker.ActiveMode.IsFarmMode() ||
                Player.ManaPercent < Menu.Item("FarmMana").GetValue<Slider>().Value)
            {
                return;
            }

            var killable =
                MinionManager.GetMinions(Spells.Q.Range)
                    .FirstOrDefault(
                        minion =>
                            !target.NetworkId.Equals(minion.NetworkId) &&
                            HealthPrediction.GetHealthPrediction(
                                minion, (int) ((Player.AttackDelay * 1000) * 2.65f + Game.Ping / 2f), 0) <= 0 &&
                            Spells.Q.GetDamage(minion) >= minion.Health);

            if (killable != null)
            {
                Spells.Q.Cast(killable);
            }
        }

        private static void Farm()
        {
            if (!Orbwalker.ActiveMode.IsFarmMode() ||
                Player.ManaPercent < Menu.Item("FarmMana").GetValue<Slider>().Value)
            {
                return;
            }

            if (Spells.Q.IsReady() && Menu.Item("FarmQ").IsActive())
            {
                if ((Menu.Item("FarmQLH").IsActive() && Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LastHit)) ||
                    ShouldWaitForMinionKill())
                {
                    return;
                }

                var target = TargetSelector.GetTarget(Spells.Q.Range, TargetSelector.DamageType.Magical, false);

                //If we can hit the target and 2 other things, then cast Q
                if (Spells.Q.CastIfWillHit(target, 3))
                {
                    return;
                }

                //  Otherwise, cast Q on the minion wave
                var minions = MinionManager.GetMinions(Player.ServerPosition, Spells.Q.Range);
                var qHit = Spells.Q.GetLineFarmLocation(minions);
                if (qHit.MinionsHit >= Menu.Item("FarmQHC").GetValue<Slider>().Value && Spells.Q.Cast(qHit.Position))
                {
                    return;
                }
            }

            if (Spells.W.IsReady() && Menu.Item("FarmW").IsActive() && Spells.W.Cast()) {}
        }

        #endregion
    }
}