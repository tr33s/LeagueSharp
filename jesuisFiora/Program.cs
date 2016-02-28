using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using jesuisFiora.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Core;
using TreeLib.Extensions;
using TreeLib.Objects;
using TreeLib.SpellData;
using Color = SharpDX.Color;
using Geometry = LeagueSharp.Common.Geometry;

namespace jesuisFiora
{
    internal static class Program
    {
        #region Static Fields

        public static Menu Menu;

        public static Orbwalking.Orbwalker Orbwalker;

        public static Color ScriptColor = new Color(255, 0, 255);

        #endregion

        #region Public Properties

        public static Spell E
        {
            get { return SpellManager.E; }
        }

        public static Spell Ignite
        {
            get { return TreeLib.Managers.SpellManager.Ignite; }
        }

        public static Spell Q
        {
            get { return SpellManager.Q; }
        }

        public static Spell R
        {
            get { return SpellManager.R; }
        }

        public static Spell W
        {
            get { return SpellManager.W; }
        }

        #endregion

        #region Properties

        private static IEnumerable<Obj_AI_Hero> Enemies
        {
            get { return HeroManager.Enemies; }
        }

        private static float FioraAutoAttackRange
        {
            get { return Orbwalking.GetRealAutoAttackRange(Player); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static List<Obj_AI_Base> QJungleMinions
        {
            get { return MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral); }
        }

        private static List<Obj_AI_Base> QLaneMinions
        {
            get { return MinionManager.GetMinions(Q.Range); }
        }

        #endregion

        #region Public Methods and Operators

        public static bool CastItems(Obj_AI_Base target)
        {
            if (Player.IsDashing() || Player.IsWindingUp)
            {
                return false;
            }

            var botrk = ItemManager.Botrk;
            if (botrk.IsValidAndReady() && botrk.Cast(target))
            {
                return true;
            }

            var cutlass = ItemManager.Cutlass;
            if (cutlass.IsValidAndReady() && cutlass.Cast(target))
            {
                return true;
            }

            var youmuus = ItemManager.Youmuus;
            if (youmuus.IsValidAndReady() && youmuus.Cast())
            {
                return true;
            }

            var units =
                MinionManager.GetMinions(385, MinionTypes.All, MinionTeam.NotAlly).Count(o => !(o is Obj_AI_Turret));
            var heroes = Player.GetEnemiesInRange(385).Count;
            var count = units + heroes;

            var tiamat = ItemManager.Tiamat;
            if (tiamat.IsValidAndReady() && count > 0 && tiamat.Cast())
            {
                return true;
            }

            var hydra = ItemManager.RavenousHydra;
            if (hydra.IsValidAndReady() && count > 0 && hydra.Cast())
            {
                return true;
            }

            var titanic = ItemManager.TitanicHydra;
            return titanic.IsValidAndReady() && count > 0 && titanic.Cast();
        }

        public static bool CastQ(Obj_AI_Hero target, FioraPassive passive, bool force = false)
        {
            if (!Q.IsReady() || !target.IsValidTarget(Q.Range))
            {
                return false;
            }

            var qPos = GetBestCastPosition(target, passive);

            if (!Q.IsInRange(qPos.Position) || qPos.Position.DistanceToPlayer() < 75)
            {
                Console.WriteLine("NOT IN RANGE");
                return false;
            }

            // cast q because we don't care
            if (!Menu.Item("QPassive").IsActive() || force)
            {
                Console.WriteLine("FORCE Q");
                return Q.Cast(qPos.Position);
            }

            // q pos under turret
            if (Menu.Item("QBlockTurret").IsActive() && qPos.Position.UnderTurret(true))
            {
                return false;
            }

            var forcePassive = Menu.Item("QPassive").IsActive();
            var passiveType = qPos.Type.ToString();

            // passive type is none, no special checks needed
            if (passiveType.Equals("None"))
            {
                //  Console.WriteLine("NO PASSIVE");
                return !forcePassive && Q.Cast(qPos.Position);
            }

            if (Menu.Item("QInVitalBlock").IsActive() && qPos.SimplePolygon.IsInside(Player.ServerPosition))
            {
                return false;
            }

            var active = Menu.Item("Q" + passiveType) != null && Menu.Item("Q" + passiveType).IsActive();

            if (!active)
            {
                return false;
            }

            if (qPos.Position.DistanceToPlayer() < 730)
            {
                return (from point in GetQPolygon(qPos.Position).Points
                    from vitalPoint in
                        qPos.Polygon.Points.OrderBy(p => p.DistanceToPlayer()).ThenByDescending(p => p.Distance(target))
                    where point.Distance(vitalPoint) < 20
                    select point).Any() && Q.Cast(qPos.Position);
            }

            Console.WriteLine("DEFAULT CAST");
            return !forcePassive && Q.Cast(qPos.Position);
        }

        public static bool CastR(Obj_AI_Base target)
        {
            return R.IsReady() && target.IsValidTarget(R.Range) && R.Cast(target).IsCasted();
        }

        public static bool CastW(Obj_AI_Base target)
        {
            if (target == null || !target.IsValidTarget(W.Range))
            {
                Console.WriteLine("CAST W");
                return W.Cast(Game.CursorPos);
            }

            var cast = W.GetPrediction(target);
            var castPos = W.IsInRange(cast.CastPosition) ? cast.CastPosition : target.ServerPosition;

            Console.WriteLine("CAST W");
            return W.Cast(castPos);
        }

        public static bool ComboR(Obj_AI_Base target)
        {
            if (Menu.Item("RComboSelected").IsActive())
            {
                var unit = TargetSelector.GetSelectedTarget();
                if (unit != null && unit.IsValid && unit.NetworkId.Equals(target.NetworkId) && CastR(target))
                {
                    return true;
                }
                return false;
            }

            if (!CastR(target))
            {
                return false;
            }

            Hud.SelectedUnit = target;
            return true;
        }

        public static void DuelistMode()
        {
            if (!Menu.Item("RCombo").IsActive() || !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Combo) ||
                !Menu.Item("RMode").GetValue<StringList>().SelectedIndex.Equals(0) || !R.IsReady() ||
                Player.CountEnemiesInRange(R.Range) == 0)

            {
                return;
            }

            var vitalCalc = Menu.Item("RKillVital").GetValue<Slider>().Value;
            foreach (var obj in
                Enemies.Where(
                    enemy =>
                        Menu.Item("Duelist" + enemy.ChampionName).IsActive() && enemy.IsValidTarget(R.Range) &&
                        GetComboDamage(enemy, vitalCalc) >= enemy.Health &&
                        enemy.Health > Player.GetSpellDamage(enemy, SpellSlot.Q) + enemy.GetPassiveDamage(1)))
            {
                if (Menu.Item("RComboSelected").IsActive())
                {
                    var unit = TargetSelector.GetSelectedTarget();
                    if (unit != null && unit.IsValid && unit.NetworkId.Equals(obj.NetworkId) && CastR(obj))
                    {
                        return;
                    }
                    return;
                }

                if (CastR(obj))
                {
                    Hud.SelectedUnit = obj;
                }

                if (Menu.Item("DuelistDraw").IsActive())
                {
                    var pos = obj.HPBarPosition;
                    Drawing.DrawText(pos.X, pos.Y - 30, System.Drawing.Color.DeepPink, "Killable!");
                }
            }
        }

        public static void Farm()
        {
            var mode = Orbwalker.ActiveMode;

            if (!Menu.Item("FarmEnabled").IsActive() || !mode.IsFarmMode())
            {
                return;
            }

            var active = Menu.Item("QFarm").IsActive() && Q.IsReady() /*&& !Q.HasManaCondition()*/&&
                         Player.ManaPercent >= Menu.Item("QFarmMana").GetValue<Slider>().Value;
            var onlyLastHit = Menu.Item("QLastHit").IsActive();

            if (!active)
            {
                return;
            }

            var laneMinions = QLaneMinions;
            var jungleMinions = QJungleMinions;

            var jungleKillable =
                jungleMinions.FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));
            if (jungleKillable != null && Q.Cast(jungleKillable).IsCasted())
            {
                return;
            }

            var jungle = jungleMinions.MinOrDefault(obj => obj.Health);
            if (!onlyLastHit && jungle != null && Q.Cast(jungle).IsCasted())
            {
                return;
            }

            var killable = laneMinions.FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            if (Menu.Item("QFarmAA").IsActive() && killable != null && killable.IsValidTarget(FioraAutoAttackRange) &&
                !Player.UnderTurret(false))
            {
                return;
            }

            if (killable != null && Q.Cast(killable).IsCasted())
            {
                return;
            }

            var lane = laneMinions.MinOrDefault(obj => obj.Health);
            if (!onlyLastHit && lane != null && Q.Cast(lane).IsCasted()) {}
        }

        public static bool Flee()
        {
            if (!Menu.Item("QFlee").IsActive())
            {
                return false;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (!Player.IsDashing() && Player.GetWaypoints().Last().Distance(Game.CursorPos) > 100)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }

            if (Q.IsReady())
            {
                Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, Q.Range + 10));
            }

            return true;
        }

        public static QPosition GetBestCastPosition(Obj_AI_Hero target, FioraPassive passive)
        {
            if (passive == null || passive.Target == null)
            {
                return new QPosition(Q.GetPrediction(target).UnitPosition);
            }

            return new QPosition(passive.CastPosition, passive.Passive, passive.Polygon, passive.SimplePolygon);
        }

        public static float GetComboDamage(Obj_AI_Hero unit)
        {
            return GetComboDamage(unit, 0);
        }

        public static float GetComboDamage(Obj_AI_Hero unit, int maxStacks)
        {
            var d = 2 * Player.GetAutoAttackDamage(unit);

            if (ItemManager.RavenousHydra.IsValidAndReady() || ItemManager.TitanicHydra.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Hydra);
            }

            if (ItemManager.Tiamat.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Tiamat);
            }

            if (ItemManager.Botrk.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Botrk);
            }

            if (ItemManager.Cutlass.IsValidAndReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Bilgewater);
            }

            if (ItemManager.Youmuus.IsValidAndReady())
            {
                d += Player.GetAutoAttackDamage(unit, true) * 2;
            }

            if (Ignite != null && Ignite.IsReady())
            {
                d += Player.GetSummonerSpellDamage(unit, Damage.SummonerSpell.Ignite);
            }

            if (Q.IsReady())
            {
                d += Player.GetSpellDamage(unit, SpellSlot.Q);
            }

            if (E.IsReady())
            {
                d += 2 * Player.GetAutoAttackDamage(unit);
            }

            if (maxStacks == 0)
            {
                if (R.IsReady())
                {
                    d += unit.GetPassiveDamage(4);
                }
                else
                {
                    d += unit.GetPassiveDamage();
                }
            }
            else
            {
                d += unit.GetPassiveDamage(maxStacks);
            }

            return (float) d;
        }

        public static Geometry.Polygon GetQPolygon(Vector3 destination)
        {
            var polygon = new Geometry.Polygon();
            for (var i = 10; i < SpellManager.QSkillshotRange; i += 10)
            {
                if (i > SpellManager.QSkillshotRange)
                {
                    break;
                }

                polygon.Add(Player.ServerPosition.Extend(destination, i));
            }

            return polygon;
        }

        public static void KillstealQ()
        {
            if (!Menu.Item("QKillsteal").IsActive())
            {
                return;
            }

            var unit =
                Enemies.FirstOrDefault(
                    o => o.IsValidTarget(Q.Range) && o.Health < Q.GetDamage(o) + o.GetPassiveDamage());
            if (unit != null)
            {
                CastQ(unit, unit.GetNearestPassive(), true);
            }
        }

        public static void KillstealW()
        {
            if (!Menu.Item("WKillsteal").IsActive())
            {
                return;
            }

            if (Menu.Item("WTurret").IsActive() && Player.UnderTurret(true))
            {
                return;
            }

            var unit =
                Enemies.FirstOrDefault(
                    o => o.IsValidTarget(W.Range) && o.Health < W.GetDamage(o) && !o.IsValidTarget(FioraAutoAttackRange));
            if (unit != null)
            {
                W.Cast(unit);
            }
        }

        public static void OrbwalkToPassive(Obj_AI_Hero target, FioraPassive passive)
        {
            if (Player.Spellbook.IsAutoAttacking)
            {
                return;
            }

            if (Menu.Item("OrbwalkAA").IsActive() && Orbwalking.CanAttack() &&
                target.IsValidTarget(FioraAutoAttackRange))
            {
                Console.WriteLine("RETURN");
                return;
            }

            if (Menu.Item("OrbwalkQ").IsActive() && Q.IsReady())
            {
                return;
            }

            if (passive == null || passive.Target == null || Menu.Item("Orbwalk" + passive.Passive) == null ||
                !Menu.Item("Orbwalk" + passive.Passive).IsActive())
            {
                return;
            }

            var pos = passive.OrbwalkPosition; //PassivePosition;
            var underTurret = Menu.Item("OrbwalkTurret").IsActive() && pos.UnderTurret(true);
            var outsideAARange = Menu.Item("OrbwalkAARange").IsActive() &&
                                 Player.Distance(pos) >
                                 FioraAutoAttackRange + 250 +
                                 (passive.Type.Equals(FioraPassive.PassiveType.UltPassive) ? 50 : 0);
            if (underTurret || outsideAARange)
            {
                return;
            }

            var path = Player.GetPath(pos);
            var point = path.Length < 3 ? pos : path.Skip(path.Length / 2).FirstOrDefault();
            //  Console.WriteLine(path.Length);
            Console.WriteLine("ORBWALK TO PASSIVE: " + Player.Distance(pos));
            Orbwalker.SetOrbwalkingPoint(target.IsMoving ? point : pos);
        }

        public static Obj_AI_Hero GetTarget(bool aaTarget = false)
        {
            var mode = Menu.Item("TargetSelector").GetValue<StringList>().SelectedIndex;

            if (aaTarget)
            {
                if (UltTarget.Target.IsValidTarget(1000))
                {
                    return UltTarget.Target;
                }

                return mode.Equals(0)
                    ? TargetSelector.GetTarget(FioraAutoAttackRange, TargetSelector.DamageType.Physical)
                    : LockedTargetSelector.GetTarget(FioraAutoAttackRange, TargetSelector.DamageType.Physical);
            }

            if (UltTarget.Target.IsValidTarget(Q.Range))
            {
                return UltTarget.Target;
            }

            return mode.Equals(0)
                ? TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical)
                : LockedTargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
        }

        #endregion

        #region Methods

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var targ = target as Obj_AI_Base;

            if (!unit.IsMe || targ == null)
            {
                return;
            }

            Orbwalker.SetOrbwalkingPoint(Vector3.Zero);
            var mode = Orbwalker.ActiveMode;

            if (mode.Equals(Orbwalking.OrbwalkingMode.None) || mode.Equals(Orbwalking.OrbwalkingMode.LastHit))
            {
                return;
            }

            var comboMode = mode.GetModeString();

            if (!E.IsActive() || (comboMode.Equals("LaneClear") && !Menu.Item("FarmEnabled").IsActive()))
            {
                return;
            }

            if (E.IsReady() && /*!E.HasManaCondition() &&*/ E.Cast())
            {
                Console.WriteLine("AFRTE");
                return;
            }

            if (ItemManager.IsActive())
            {
                CastItems(targ);
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var targ = args.Target as Obj_AI_Hero;

            if (!args.Unit.IsMe || targ == null)
            {
                return;
            }

            if (!E.IsActive() || /*E.HasManaCondition() ||*/ !E.IsReady() || !Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            if (!targ.IsFacing(Player) && targ.Distance(Player) >= FioraAutoAttackRange - 10)
            {
                Console.WriteLine("BEFORE");
                E.Cast();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Q.IsReady())
            {
                var vitalCircle = Menu.Item("QVitalDraw").GetValue<Circle>();
                if (vitalCircle.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, vitalCircle.Radius, vitalCircle.Color);
                }

                var qCircle = Menu.Item("QDraw").GetValue<Circle>();
                if (qCircle.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, qCircle.Radius, qCircle.Color);
                }
            }

            foreach (var circle in from spell in new[] { 1, 3 }
                let circle = Menu.Item(spell + "Draw").GetValue<Circle>()
                where circle.Active && Player.Spellbook.GetSpell((SpellSlot) spell).IsReady()
                select circle)
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static void Game_OnGameLoad(object obj)
        {
            if (!Player.IsChampion("Fiora"))
            {
                return;
            }

            Bootstrap.Initialize();

            Menu = new Menu("jesuisFiora", "je suis Fiora", true);
            Menu.SetFontStyle(FontStyle.Regular, ScriptColor);

            Orbwalker = Menu.AddOrbwalker();

            var hr = Menu.SubMenu("Orbwalker").Item("HoldPosRadius").GetValue<Slider>();
            if (hr.Value < 60)
            {
                hr.Value = 60;
                Menu.SubMenu("Orbwalker").Item("HoldPosRadius").SetValue(hr);
            }

            var spells = Menu.AddMenu("Spells", "Spells");
            var passive = Menu.AddMenu("Passive", "Vital Settings");
            var wMenu = Menu.AddMenu("W", "W (SpellBlock)");

            var orbwalker = passive.AddMenu("Orbwalker", "Orbwalk Vital");

            orbwalker.AddKeyBind("OrbwalkPassive", "Orbwalk to Target Vital", 'N', KeyBindType.Toggle, true);
            orbwalker.Item("OrbwalkPassive").SetTooltip("Attempt to orbwalk to AA enemy vital.", ScriptColor);

            orbwalker.AddBool("OrbwalkCombo", "In Combo");
            orbwalker.Item("OrbwalkCombo").SetTooltip("Only orbwalk to vital in Combo mode.", ScriptColor);

            orbwalker.AddBool("OrbwalkHarass", "In Harass");
            orbwalker.Item("OrbwalkHarass").SetTooltip("Only orbwalk to vital in Harass mode.", ScriptColor);

            orbwalker.AddBool("OrbwalkPrepassive", "Orbwalk PreVital");
            orbwalker.Item("OrbwalkPrepassive")
                .SetTooltip("Orbwalk to a vital before it has been identified.", ScriptColor);

            orbwalker.AddBool("OrbwalkUltPassive", "Orbwalk Ultimate Vital");
            orbwalker.Item("OrbwalkUltPassive").SetTooltip("Orbwalk to ultimate vitals.", ScriptColor);

            orbwalker.AddBool("OrbwalkPassiveTimeout", "Orbwalk Near Timeout Vital");
            orbwalker.Item("OrbwalkPassiveTimeout")
                .SetTooltip("Orbwalk to  to vital as it is being timed out.", ScriptColor);

            orbwalker.AddBool("OrbwalkSelected", "Only Selected Target", true);
            orbwalker.Item("OrbwalkSelected")
                .SetTooltip("Target must be manually left clicked to orbwalk to vitals.", ScriptColor);

            orbwalker.AddBool("OrbwalkTurret", "Block Under Turret", false);
            orbwalker.Item("OrbwalkTurret").SetTooltip("In order to avoid walking under turrets.", ScriptColor);

            orbwalker.AddBool("OrbwalkQ", "Only if Q Down", false);
            orbwalker.Item("OrbwalkQ").SetTooltip("To avoid orbwalking to a vital that will be Q'ed to.", ScriptColor);

            orbwalker.AddBool("OrbwalkAARange", "Only in AA Range", false);
            orbwalker.Item("OrbwalkAARange").SetTooltip("Only orbwalk to vital if it is in AA range.", ScriptColor);

            orbwalker.AddBool("OrbwalkAA", "Only if not able to AA", false);
            orbwalker.Item("OrbwalkAA")
                .SetTooltip("Only orbwalk to vital if not able to AA, in order to avoid loss of dps.", ScriptColor);

            var qVital = passive.AddMenu("QVital", "Q Vital");
            qVital.AddBool("QPassive", "Only Q to Vitals", true);
            qVital.Item("QPassive").SetTooltip("Attempt to only Q to Fiora's vital passive.", ScriptColor);

            qVital.AddBool("QUltPassive", "Q to Ultimate Vital");
            qVital.Item("QUltPassive").SetTooltip("Q to ultimate vital passive.", ScriptColor);

            qVital.AddBool("QPrepassive", "Q to PreVital", false);
            qVital.Item("QPrepassive")
                .SetTooltip("Attempt to Q to vital before it has been identified. May not proc vital.", ScriptColor);

            qVital.AddBool("QPassiveTimeout", "Q to Near Timeout Vital");
            qVital.Item("QPassiveTimeout")
                .SetTooltip("Q to vital as it is being timed out. May not proc vital.", ScriptColor);

            qVital.AddBool("QInVitalBlock", "Block Q inside Vital Polygon");
            qVital.Item("QInVitalBlock").SetTooltip("Block Q if player is inside of enemy vital polygon.", ScriptColor);

            passive.AddBool("DrawCenter", "Draw Vital Center");
            passive.Item("DrawCenter").SetTooltip("Draw the center of vital polygon. No FPS drops.", ScriptColor);

            passive.AddBool("DrawPolygon", "Draw Vital Polygon", false);
            passive.Item("DrawPolygon").SetTooltip("Draw the vital polygon. Possibly causes FPS drops.", ScriptColor);

            passive.AddSlider("SectorMaxRadius", "Vital Polygon Range", 310, 300, 400);
            passive.Item("SectorMaxRadius")
                .SetTooltip("The max range of vital polygon. Draw polygon to understand what this is.", ScriptColor);

            passive.AddSlider("SectorAngle", "Vital Polygon Angle", 70, 60, 90);
            passive.Item("SectorAngle")
                .SetTooltip("The angle of vital polygon. Draw polygon to understand what this is.", ScriptColor);

            var qMenu = spells.AddMenu("Q", "Q");
            qMenu.AddBool("QCombo", "Use in Combo");
            qMenu.AddBool("QHarass", "Use in Harass");
            qMenu.AddSlider("QRangeDecrease", "Decrease Q Range", 10, 0, 150);
            Q.Range = 750 - qMenu.Item("QRangeDecrease").GetValue<Slider>().Value;
            qMenu.Item("QRangeDecrease").ValueChanged += (sender, eventArgs) =>
            {
                Q.Range = 750 - eventArgs.GetNewValue<Slider>().Value;
                var qDraw = Menu.Item("QDraw");
                if (qDraw == null)
                {
                    return;
                }
                var qCircle = qDraw.GetValue<Circle>();
                qDraw.SetValue(new Circle(qCircle.Active, qCircle.Color, Q.Range));
            };

            qMenu.AddBool("QBlockTurret", "Block Q Under Turret", false);
            qMenu.Item("QBlockTurret").SetTooltip("Don't Q under turret in combo/harass.", ScriptColor);

            qMenu.AddKeyBind("QFlee", "Q Flee", 'T');
            qMenu.Item("QFlee").SetTooltip("Flees towards cursor position.", ScriptColor);
            //qMenu.AddInfo("FleeInfo", " --> Flees towards cursor position.", ScriptColor);

            qMenu.AddBool("QKillsteal", "Use for Killsteal");

            var wSpells = wMenu.AddMenu("BlockSpells", "Blocked Spells");
            wMenu.AddKeyBind("WSpells", "Enabled", 'U', KeyBindType.Toggle, true);

            wMenu.AddList("WMode", "W Spellblock to: ", new[] { "Spell Caster", "Target" });
            wMenu.Item("WMode").SetTooltip("TR", ScriptColor);
            wMenu.AddBool("WKillsteal", "Use for Killsteal");
            wMenu.AddBool("WTurret", "Block W Under Enemy Turret", false);

            SpellBlock.Initialize(wSpells);
            Dispeller.Initialize(wSpells);

            var eMenu = spells.AddMenu("E", "E");
            eMenu.AddBool("ECombo", "Use in Combo");
            eMenu.AddBool("EHarass", "Use in Harass");

            var rMenu = spells.AddMenu("R", "R");

            var duelistMenu = rMenu.AddMenu("Duelist Champion", "Duelist Mode Champions");
            foreach (var enemy in Enemies)
            {
                duelistMenu.AddBool("Duelist" + enemy.ChampionName, "Use on " + enemy.ChampionName);
            }

            rMenu.AddBool("RCombo", "Use R");

            rMenu.AddList("RMode", "Cast Mode", new[] { "Duelist", "Combo" });
            rMenu.Item("RMode")
                .SetTooltip("Duelist: Only cast when killable. Combo: Cast during normal combo.", ScriptColor);

            rMenu.AddKeyBind("RToggle", "Toggle Mode", 'L');
            rMenu.Item("RToggle").SetTooltip("Toggles cast mode between Duelist and Combo.", ScriptColor);

            rMenu.Item("RToggle").ValueChanged += (sender, eventArgs) =>
            {
                if (!eventArgs.GetNewValue<KeyBind>().Active)
                {
                    return;
                }
                var mode = Menu.Item("RMode");
                var index = mode.GetValue<StringList>().SelectedIndex == 0 ? 1 : 0;
                mode.SetValue(new StringList(new[] { "Duelist", "Combo" }, index));
            };

            rMenu.AddSlider("RKillVital", "Duelist Mode Min Vitals", 2, 0, 4);
            rMenu.Item("RKillVital").SetTooltip("Used for damage calculation in Duelist Mode", ScriptColor);

            rMenu.AddBool("RComboSelected", "Use R Selected on Selected Unit Only");
            rMenu.Item("RComboSelected")
                .SetTooltip("Only cast R when enemy has been left clicked or selected.", ScriptColor);

            var items = spells.AddMenu("Items", "Items");
            items.AddBool("ItemsCombo", "Use in Combo");
            items.AddBool("ItemsHarass", "Use in Harass");

            spells.AddSlider("ManaHarass", "Harass Min Mana Percent", 40);

            var farm = Menu.AddMenu("Farm", "Farm");

            var qFarm = farm.AddMenu("Farm", "Q");
            qFarm.AddBool("QFarm", "Use Q in Farm");
            qFarm.AddBool("QLastHit", "Q Last Hit (Only Killable)", false);
            qFarm.AddBool("QFarmAA", "Only Q out of AA Range", false);
            qFarm.AddSlider("QFarmMana", "Q Min Mana Percent", 40);

            var eFarm = farm.AddMenu("E", "E");
            eFarm.AddBool("ELaneClear", "Use in LaneClear");

            farm.AddKeyBind("FarmEnabled", "Farm Enabled", 'J', KeyBindType.Toggle);
            farm.Item("FarmEnabled").SetTooltip("Enabled in LastHit and LaneClear mode.", ScriptColor);

            farm.AddBool("ItemsLaneClear", "Use Items in LaneClear");

            var draw = Menu.AddMenu("Drawing", "Drawing");

            draw.AddCircle(
                "QVitalDraw", "Draw Q Vital Range", System.Drawing.Color.Purple, SpellManager.QSkillshotRange, false);
            draw.Item("QVitalDraw")
                .SetTooltip(
                    "Must be in this range to hit vital. If force vital enabled, then it can only cast Q to target in this range.",
                    ScriptColor);

            draw.AddCircle("QDraw", "Draw Q Max Range", System.Drawing.Color.Purple, Q.Range, false);
            draw.Item("QDraw").SetTooltip("The max range that Q can be cast and hit the target.", ScriptColor);

            draw.AddCircle("1Draw", "Draw W", System.Drawing.Color.DeepPink, W.Range, false);
            draw.AddCircle("3Draw", "Draw R", System.Drawing.Color.White, R.Range, false);
            draw.AddBool("DuelistDraw", "Duelist Mode: Killable Target");
            draw.AddBool("WPermashow", "Permashow W Spellblock");
            draw.AddBool("RPermashow", "Permashow R Mode");
            draw.AddBool("FarmPermashow", "Permashow Farm Enabled");
            draw.AddBool("OrbwalkPermashow", "Permashow Orbwalk Vital");

            if (draw.Item("WPermashow").IsActive())
            {
                wMenu.Item("WSpells").Permashow(true, "W SpellBlock", ScriptColor);
            }

            draw.Item("WPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    wMenu.Item("WSpells").Permashow(eventArgs.GetNewValue<bool>(), "W SpellBlock", ScriptColor);
                };

            if (draw.Item("RPermashow").IsActive())
            {
                rMenu.Item("RMode").Permashow(true, null, ScriptColor);
            }

            draw.Item("RPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    rMenu.Item("RMode").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor);
                };

            if (draw.Item("FarmPermashow").IsActive())
            {
                farm.Item("FarmEnabled").Permashow(true, null, ScriptColor);
            }

            draw.Item("FarmPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    farm.Item("FarmEnabled").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor);
                };

            if (draw.Item("OrbwalkPermashow").IsActive())
            {
                orbwalker.Item("OrbwalkPassive").Permashow(true, null, ScriptColor);
            }

            draw.Item("OrbwalkPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    orbwalker.Item("OrbwalkPassive").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor);
                };

            var dmg = draw.AddMenu("DamageIndicator", "Damage Indicator");
            dmg.AddBool("DmgEnabled", "Draw Damage Indicator");
            dmg.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
            dmg.AddCircle("FillColor", "Damage Color", System.Drawing.Color.HotPink);
            dmg.AddBool("Killable", "Killable Text");

            var misc = Menu.AddMenu("Misc", "Misc");

            /*ManaManager.Initialize(misc);
            Q.SetManaCondition(ManaManager.ManaMode.Combo, 5);
            Q.SetManaCondition(ManaManager.ManaMode.Harass, 5);
            Q.SetManaCondition(ManaManager.ManaMode.Farm, 30);
            E.SetManaCondition(ManaManager.ManaMode.Combo, 15);
            E.SetManaCondition(ManaManager.ManaMode.Harass, 15);
            E.SetManaCondition(ManaManager.ManaMode.Farm, 40);
            R.SetManaCondition(ManaManager.ManaMode.Combo, 10);
            */

            misc.AddList("TargetSelector", "Target Selector: ", new[] { "Normal", "Locked" });
            misc.Item("TargetSelector").SetTooltip("Locked TS attempts to stick to the same target.", ScriptColor);
            misc.AddInfo("TSInfo", "Locked TS attempts to lock to the same target.", ScriptColor);
            misc.AddBool("Sounds", "Sounds");

            Menu.AddInfo("Info", "By Trees and Lilith!", ScriptColor);
            Menu.AddToMainMenu();

            if (Menu.Item("Sounds").IsActive())
            {
                new SoundObject(Resources.OnLoad).Play();
            }

            DamageIndicator.DamageToUnit = GetComboDamage;
            PassiveManager.Initialize();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat(
                "<font color=\"{0}\">jesuisFiora Loaded!</font>", System.Drawing.Color.DeepPink.ToHexString());
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetOrbwalkingPoint(Vector3.Zero);

            if (Player.IsDead || Flee())
            {
                return;
            }

            KillstealQ();
            KillstealW();
            DuelistMode();
            Farm();

            if (Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            if (!Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            var aaTarget = GetTarget(true);
            var passive = new FioraPassive();
            if (aaTarget.IsValidTarget())
            {
                passive = aaTarget.GetNearestPassive();
                if (Menu.Item("OrbwalkPassive").IsActive() &&
                    Menu.Item("Orbwalk" + Orbwalker.ActiveMode.GetModeString()).IsActive())
                {
                    var selTarget = TargetSelector.SelectedTarget;
                    //Console.WriteLine("START ORBWALK TO PASSIVE");
                    if (!Menu.Item("OrbwalkSelected").IsActive() ||
                        (selTarget != null && selTarget.NetworkId.Equals(aaTarget.NetworkId)))
                    {
                        OrbwalkToPassive(aaTarget, passive);
                    }
                }
                Orbwalker.ForceTarget(aaTarget);
            }

            var target = GetTarget();
            if (!target.IsValidTarget(W.Range))
            {
                return;
            }

            var vital = aaTarget != null && target.NetworkId.Equals(aaTarget.NetworkId)
                ? passive
                : target.GetNearestPassive();

            if (Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Mixed) &&
                Player.ManaPercent < Menu.Item("ManaHarass").GetValue<Slider>().Value)
            {
                return;
            }

            if (R.IsActive() /*&& !R.HasManaCondition()*/&&
                Menu.Item("RMode").GetValue<StringList>().SelectedIndex.Equals(1) && ComboR(target))
            {
                return;
            }

            if (Q.IsActive()) // && !Q.HasManaCondition())
            {
                if (target.IsValidTarget(FioraAutoAttackRange) && !Orbwalking.IsAutoAttack(Player.LastCastedSpellName()))
                {
                    return;
                }

                if (target.ChampionName.Equals("Poppy") && target.HasBuff("poppywzone"))
                {
                    return;
                }

                CastQ(target, vital);
                /*  var path = target.GetWaypoints();
                if (path.Count == 1 || Player.Distance(target) < 700)
                {
                    CastQ(target);
                    return;
                }

                var d = target.Distance(path[1]);
                var d2 = Player.Distance(path[1]);
                var t = d / target.MoveSpeed;
                var dT = Q.Delay + Game.Ping / 2000f - t;
                if ((dT > .2f || (d2 < 690 && dT > -1)) && CastQ(target))
                {
                    //  Console.WriteLine("{0} {1}", dT, d2);
                }*/
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            //LeagueSharp.SDK.Events.OnLoad += Game_OnGameLoad;
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;

            if (unit == null || !unit.IsValid)
            {
                return;
            }

            if (unit.IsMe && args.Slot.Equals(SpellSlot.E))
            {
                Orbwalking.ResetAutoAttackTimer();
                return;
            }

            if (!unit.IsEnemy || !Menu.Item("WSpells").IsActive() || !W.IsReady())
            {
                return;
            }

            // spell handled by evade
            if (SpellDatabase.GetByName(args.SData.Name) != null)
            {
                Console.WriteLine("EVADE PROCESS SPELL RETURN");
                return;
            }

            Console.WriteLine("({0}) {1}", args.Slot, args.SData.Name);
            if (!SpellBlock.Contains(unit, args))
            {
                return;
            }

            var castUnit = unit;
            var type = args.SData.TargettingType;

            Console.WriteLine("Type: {0} Range: {1} Radius: {2}", type, args.SData.CastRange, args.SData.CastRadius);
            Console.WriteLine("Distance: " + args.End.DistanceToPlayer());

            if (!unit.IsValidTarget() || Menu.Item("WMode").GetValue<StringList>().SelectedIndex == 1)
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target == null || !target.IsValidTarget(W.Range))
                {
                    target = TargetSelector.GetTargetNoCollision(W);
                }

                if (target != null && target.IsValidTarget(W.Range))
                {
                    castUnit = target;
                }
            }

            if (type.IsSkillShot())
            {
                if (unit.ChampionName.Equals("Bard") && args.End.DistanceToPlayer() < 300)
                {
                    Utility.DelayAction.Add(400 + (int) (unit.Distance(Player) / 7f), () => CastW(castUnit));
                }
                else if (unit.ChampionName.Equals("Riven") && args.End.DistanceToPlayer() < 260)
                {
                    Console.WriteLine("RIVEN");
                    CastW(castUnit);
                }
                else if (args.End.DistanceToPlayer() < 60)
                {
                    CastW(castUnit);
                }
            }
            if (type.IsTargeted() && args.Target != null)
            {
                if (!args.Target.IsMe ||
                    (args.Target.Name.Equals("Barrel") && args.Target.DistanceToPlayer() > 200 &&
                     args.Target.DistanceToPlayer() < 400))
                {
                    return;
                }

                if (Menu.Item("WTurret").IsActive() && Player.UnderTurret(true))
                {
                    return;
                }

                if (unit.ChampionName.Equals("Nautilus") ||
                    (unit.ChampionName.Equals("Caitlyn") && args.Slot.Equals(SpellSlot.R)))
                {
                    var d = unit.DistanceToPlayer();
                    var travelTime = d / args.SData.MissileSpeed;
                    var delay = travelTime * 1000 - W.Delay + 150;
                    Console.WriteLine("TT: " + travelTime + " " + delay);
                    Utility.DelayAction.Add((int) delay, () => CastW(castUnit));
                    return;
                }

                CastW(castUnit);
            }
            else if (type.Equals(SpellDataTargetType.LocationAoe) && args.End.DistanceToPlayer() < args.SData.CastRadius)
            {
                // annie moving tibbers
                if (unit.ChampionName.Equals("Annie") && args.Slot.Equals(SpellSlot.R))
                {
                    return;
                }
                CastW(castUnit);
            }
            else if (type.Equals(SpellDataTargetType.Cone) && args.End.DistanceToPlayer() < args.SData.CastRadius)
            {
                CastW(castUnit);
            }
            else if (type.Equals(SpellDataTargetType.SelfAoe) || type.Equals(SpellDataTargetType.Self))
            {
                var d = args.End.Distance(Player.ServerPosition);
                var p = args.SData.CastRadius > 5000 ? args.SData.CastRange : args.SData.CastRadius;
                Console.WriteLine(d + " " + " " + p);
                if (d < p)
                {
                    Console.WriteLine("CAST");
                    CastW(castUnit);
                }
            }
        }

        #endregion
    }
}