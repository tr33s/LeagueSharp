using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = SharpDX.Color;

namespace KindredSpirits
{
    internal class Program
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Color ScriptColor = new Color(0, 0, 255);

        public static List<Obj_AI_Hero> Enemies
        {
            get { return HeroManager.Enemies; }
        }

        public static List<Obj_AI_Hero> Allies
        {
            get { return HeroManager.Allies; }
        }

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static float AutoAttackRange
        {
            get { return Orbwalking.GetRealAutoAttackRange(Player); }
        }

        private static List<Obj_AI_Base> LaneMinions
        {
            get { return MinionManager.GetMinions(SpellManager.Q.Range); }
        }

        private static List<Obj_AI_Base> JungleMinions
        {
            get { return MinionManager.GetMinions(SpellManager.Q.Range, MinionTypes.All, MinionTeam.Neutral); }
        }

        private static Obj_AI_Hero SelectedTarget
        {
            get { return Hud.SelectedUnit as Obj_AI_Hero; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.IsChampion("Kindred"))
            {
                return;
            }

            Menu = new Menu("KindredSpirits", "KindredSpirits", true);
            Menu.SetFontStyle(FontStyle.Regular, ScriptColor);

            Orbwalker = Menu.AddOrbwalker();
            Menu.AddTargetSelector();

            var spells = Menu.AddMenu("Spells", "Spells");

            var qMenu = spells.AddMenu("Q", "Q");
            qMenu.AddBool("QCombo", "Use in Combo");
            qMenu.AddBool("QHarass", "Use in Harass");
            qMenu.AddBool("QSafety", "Q Safety Check");
            qMenu.AddBool("QKiteMachine", "Q KiteMachine");
            qMenu.AddInfo("KiteInfo", " --> Q's towards cursor position if enemy will be hit.", ScriptColor);
            qMenu.AddBool("QAlwaysKite", "Q Always to Cursor", false);
            qMenu.AddBool("QGapClose", "AntiGapclose with Q");
            qMenu.AddBool("QKillsteal", "Use for Killsteal");

            var wMenu = spells.AddMenu("W", "W");
            wMenu.AddBool("WCombo", "Use in Combo");
            wMenu.AddBool("WHarass", "Use in Harass");

            var eMenu = spells.AddMenu("E", "E");
            eMenu.AddBool("ECombo", "Use in Combo");
            eMenu.AddBool("EHarass", "Use in Harass");
            eMenu.AddBool("EBeforeAttack", "Only Use E Before Attack", false);
            eMenu.AddInfo("BeforeAttackInfo", " --> When enemy is close to leaving AA range.", ScriptColor);
            eMenu.AddBool("ESelectedTarget", "Only E Selected Target", false);

            var rMenu = spells.AddMenu("R", "R");

            var savingMenu = rMenu.AddMenu("SavingMode", "Saving Spirits Settings");
            var allyMenu = savingMenu.AddMenu("RAlly", "Allied Champions");

            foreach (var ally in Allies.Where(a => !a.IsMe))
            {
                allyMenu.AddBool("R" + ally.ChampionName, "Use on " + ally.ChampionName);
                allyMenu.AddSlider("RHP" + ally.ChampionName, "Health Percent", 15);
            }

            savingMenu.AddSlider("SavingAllies", "Minimum Allies In Range", 0, 0, 5);
            savingMenu.AddSlider("SavingEnemies", "Maximum Enemies In Range", 0, 0, 5);

            rMenu.AddBool("RCombo", "Use R");
            rMenu.AddSlider("RSelf", "Self Health Percent", 15);

            var items = spells.AddMenu("Items", "Items");
            items.AddBool("ItemsCombo", "Use in Combo");
            items.AddBool("ItemsHarass", "Use in Harass");

            spells.AddBool("IgniteKillsteal", "Ignite Killsteal");
            spells.AddBool("SmiteKillsteal", "Smite Killsteal");
            spells.AddSlider("ManaHarass", "Harass Min Mana Percent", 40);

            var farm = Menu.AddMenu("Farm", "Farm");
            farm.AddBool("QLaneClear", "Q LaneClear");
            farm.AddBool("QJungleClear", "Q Jungle Clear");
            farm.AddSlider("QFarmMana", "Q Min Mana Percent", 40);
            farm.AddBool("WLaneClear", "Farm with W");
            farm.AddBool("EJungleClear", "E Jungle Clear");

            farm.AddKeyBind("FarmEnabled", "Farm Enabled", 'J', KeyBindType.Toggle, true);
            farm.AddInfo("FarmInfo", " --> Enabled in LaneClear and LastHit", ScriptColor);
            farm.AddBool("ItemsLaneClear", "Use Items in LaneClear");

            var flee = Menu.AddMenu("Flee", "Flee");
            flee.AddInfo("FleeInfo", " --> Flees towards cursor position.", ScriptColor);
            flee.AddKeyBind("Flee", "Flee", 'T');
            flee.AddBool("FleeQ", "Use Q");
            flee.AddBool("FleeW", "Use W");
            flee.AddBool("FleeMove", "Move to Cursor Position");

            var draw = Menu.AddMenu("Drawing", "Drawing");
            draw.AddCircle("0Draw", "Draw Q", System.Drawing.Color.Purple, SpellManager.Q.Range);
            draw.AddCircle("1Draw", "Draw W", System.Drawing.Color.DeepPink, SpellManager.W.Range);
            draw.AddCircle("2Draw", "Draw E", System.Drawing.Color.Blue, SpellManager.E.Range);
            draw.AddCircle("3Draw", "Draw R", System.Drawing.Color.White, SpellManager.R.Range);
            draw.AddBool("FarmPermashow", "Permashow Farm Enabled");

            if (draw.Item("FarmPermashow").IsActive())
            {
                farm.Item("FarmEnabled").Permashow(true, null, ScriptColor);
            }

            draw.Item("FarmPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    farm.Item("FarmEnabled").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor);
                };


            var dmg = draw.AddMenu("DamageIndicator", "Damage Indicator");
            dmg.AddBool("DmgEnabled", "Draw Damage Indicator");
            dmg.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
            dmg.AddCircle("FillColor", "Damage Color", System.Drawing.Color.Blue);
            dmg.AddBool("Killable", "Killable Text");

            PassiveManager.Initialize();

            //Menu.AddBool("Sounds", "Sounds");
            Menu.AddInfo("Info", "By Trees and Lilith!", ScriptColor);
            Menu.AddToMainMenu();

            /*if (Menu.Item("Sounds").IsActive())
            {
                new SoundObject(Resources.OnLoad).Play();
            }*/

            DamageIndicator.DamageToUnit = GetComboDamage;
            UltimateManager.Initialize();

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat(
                "<font color=\"{0}\">Kindred Spirits Loaded!</font>", System.Drawing.Color.Blue.ToHexString());
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var circle in from spell in new[] { 0, 1, 2, 3 }
                let circle = Menu.Item(spell + "Draw").GetValue<Circle>()
                where circle.Active && Player.Spellbook.GetSpell((SpellSlot) spell).IsReady()
                select circle)
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        public static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!Orbwalker.ActiveMode.IsComboMode() ||
                !Menu.Item("Items" + Orbwalker.ActiveMode.GetModeString()).IsActive())
            {
                return;
            }

            var targ = target as Obj_AI_Hero;
            if (unit.IsMe && targ != null && targ.IsValid)
            {
                CastItems(targ);
            }
        }

        public static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var target = args.Target as Obj_AI_Base;
            if (target == null || !target.IsValid || !Orbwalker.ActiveMode.IsComboMode() ||
                !Menu.Item("E" + Orbwalker.ActiveMode.GetModeString()).IsActive() || !SpellManager.E.IsReady())
            {
                return;
            }

            if (Player.Distance(args.Target) > AutoAttackRange - 100 && SpellManager.E.IsInRange(target))
            {
                if (!Menu.Item("ESelectedTarget").IsActive())
                {
                    CastE(target);
                }
                else
                {
                    var selected = SelectedTarget;
                    if (!selected.IsValidTarget() || !selected.Equals(target))
                    {
                        return;
                    }

                    CastE(target);
                }
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead || Flee())
            {
                return;
            }

            if (Killsteal())
            {
                return;
            }

            Farm();

            var target = TargetSelector.GetTarget(SpellManager.Q.Range + 500, TargetSelector.DamageType.Physical);

            if (target == null || !target.IsValidTarget(SpellManager.W.Range))
            {
                return;
            }

            if (!Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            var mode = Orbwalker.ActiveMode.GetModeString();
            var qCombo = Menu.Item("Q" + mode).IsActive() && SpellManager.Q.IsReady();
            var wCombo = Menu.Item("W" + mode).IsActive() && SpellManager.W.IsReady();
            var eCombo = Menu.Item("E" + mode).IsActive() && SpellManager.E.IsReady() &&
                         target.IsValidTarget(SpellManager.E.Range);

            if (mode.Equals("Harass") && Player.ManaPercent < Menu.Item("ManaHarass").GetValue<Slider>().Value)
            {
                return;
            }

            if (Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            if (eCombo && !Menu.Item("EBeforeAttack").IsActive() && SpellManager.E.CastOnUnit(target))
            {
                if (!Menu.Item("ESelectedTarget").IsActive() && CastE(target))
                {
                    return;
                }
                var selected = SelectedTarget;
                if (!selected.IsValidTarget() || !selected.Equals(target))
                {
                    return;
                }

                if (CastE(target))
                {
                    return;
                }
            }

            if (wCombo && CastW(target))
            {
                return;
            }

            if (qCombo && CastQ(target, Menu.Item("QKiteMachine").IsActive(), Menu.Item("QAlwaysKite").IsActive())) {}
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (Menu.Item("QGapCloser").IsActive() && SpellManager.Q.IsReady() &&
                gapcloser.End.Distance(Player.ServerPosition) < 300)
            {
                var d = gapcloser.End.Distance(Player.ServerPosition);
                var castPos = gapcloser.End.Extend(Player.ServerPosition, d + SpellManager.Q.Range - 500);
                SpellManager.Q.Cast(castPos);
            }
        }

        public static void Farm()
        {
            var mode = Orbwalker.ActiveMode;
            if (!Menu.Item("FarmEnabled").IsActive() || !mode.Equals(Orbwalking.OrbwalkingMode.LaneClear))
            {
                return;
            }

            if (Menu.Item("WLaneClear").IsActive() && SpellManager.W.IsReady() && (JungleMinions.Count > 0 || LaneMinions.Count > 0))
            {
                    SpellManager.W.Cast();
            }

            if (JungleClear())
            {
                return;
            }
 
            if (!SpellManager.Q.IsReady() || !Menu.Item("QLaneClear").IsActive() ||
                Player.ManaPercent < Menu.Item("QFarmMana").GetValue<Slider>().Value)
            {
                return;
            }

            var laneMinions = LaneMinions;
            var killable = laneMinions.FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            if (killable != null && CastQ(killable))
            {
                return;
            }

            var lane = laneMinions.MinOrDefault(obj => obj.Health);
            if (lane != null && CastQ(lane)) {}
        }

        public static bool JungleClear()
        {
            var jungleMinions = JungleMinions;
            var qClear = SpellManager.Q.IsReady() && Menu.Item("QJungleClear").IsActive() &&
                         Player.ManaPercent >= Menu.Item("QFarmMana").GetValue<Slider>().Value;
            var eClear = SpellManager.E.IsReady() && Menu.Item("EJungleClear").IsActive();
            var activeSpell = SpellManager.Q;

            if (!qClear)
            {
                if (eClear)
                {
                    activeSpell = SpellManager.E;
                }
                else
                {
                    return false;
                }
            }

            var jungleKillable =
                jungleMinions.FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, activeSpell.Slot));
            if (jungleKillable != null)
            {
                if (activeSpell.Slot.Equals(SpellSlot.Q) && CastQ(jungleKillable, true))
                {
                    return true;
                }

                return activeSpell.Cast(jungleKillable).IsCasted();
            }


            var jungle = jungleMinions.MaxOrDefault(obj => obj.Health);
            if (jungle == null)
            {
                return false;
            }
            if (activeSpell.Slot.Equals(SpellSlot.Q) && CastQ(jungle, true))
            {
                return true;
            }
            return activeSpell.CastOnUnit(jungle);
        }

        public static bool CastQ(Obj_AI_Base target, bool kiteMachine = false, bool forceToCursor = false)
        {
            if (forceToCursor)
            {
                SpellManager.Q.Cast(Game.CursorPos);
            }

            if (!target.IsValidTarget(SpellManager.Q.Range))
            {
                return false;
            }

            var cast = SpellManager.Q.GetPrediction(target);

            if (kiteMachine)
            {
                //var d = Player.Distance(cast.UnitPosition);
                //var d2 = cast.UnitPosition.Distance(Game.CursorPos);
                var castPos = target.ServerPosition.Extend(Game.CursorPos, 500);
                var d = Player.Distance(target);
                if (d < 500)
                {
                    SpellManager.Q.Cast(castPos);
                }
            }

            if (target.IsValidTarget(500))
            {
                return SpellManager.Q.Cast(Player.ServerPosition);
            }


            if (Menu.Item("QSafety").IsActive() && !cast.CastPosition.IsSafeQPosition())
            {
                return false;
            }

            return cast.Hitchance >= HitChance.High && SpellManager.Q.Cast(cast.CastPosition);
        }

        public static bool CastW(Obj_AI_Base target)
        {
            return target.IsValidTarget(SpellManager.W.Range - 50) && SpellManager.W.Cast(target).IsCasted();
        }

        public static bool CastE(Obj_AI_Base target)
        {
            return target.IsValidTarget(SpellManager.E.Range) && SpellManager.E.CastOnUnit(target);
        }

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

            return false;
        }

        public static bool Flee()
        {
            if (!Menu.Item("Flee").IsActive())
            {
                return false;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (Menu.Item("FleeQ").IsActive() && SpellManager.Q.IsReady())
            {
                var pos = Player.ServerPosition.Extend(Game.CursorPos, SpellManager.Q.Range + 10);
                if (SpellManager.Q.Cast(pos))
                {
                    return true;
                }
            }

            if (!SpellManager.Q.IsReady() && Menu.Item("FleeW").IsActive() && SpellManager.W.IsReady() && SpellManager.W.Cast())
            {
                return true;
            }

            if (Menu.Item("FleeMove").IsActive() && !Player.IsDashing() &&
                Player.GetWaypoints().Last().Distance(Game.CursorPos) > 100)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                return true;
            }

            return false;
        }

        public static bool Killsteal()
        {
            if (AutoIgnite())
            {
                return true;
            }

            if (AutoSmite())
            {
                return true;
            }

            if (KillstealQ())
            {
                return true;
            }

            return false;
        }

        public static bool AutoIgnite()
        {
            if (!Menu.Item("IgniteKillsteal").IsActive() || SpellManager.Ignite == null ||
                !SpellManager.Ignite.IsReady())
            {
                return false;
            }

            var unit =
                Enemies.FirstOrDefault(
                    o =>
                        o.IsValidTarget(SpellManager.Ignite.Range) &&
                        o.Health < Player.GetSummonerSpellDamage(o, Damage.SummonerSpell.Ignite));
            return unit != null && unit.IsValid && SpellManager.Ignite.CastOnUnit(unit);
        }

        public static bool AutoSmite()
        {
            if (!Menu.Item("SmiteKillsteal").IsActive() || SpellManager.Smite == null || !SpellManager.Smite.IsReady() ||
                !SpellManager.Smite.IsCastableOnChampion())
            {
                return false;
            }
            var unit =
                Enemies.FirstOrDefault(
                    o =>
                        o.IsValidTarget(SpellManager.Smite.Range) &&
                        o.Health < Player.GetSummonerSpellDamage(o, Damage.SummonerSpell.Smite));
            return unit != null && unit.IsValid && SpellManager.Smite.CastOnUnit(unit);
        }

        public static bool KillstealQ()
        {
            if (!Menu.Item("QKillsteal").IsActive() || !SpellManager.Q.IsReady())
            {
                return false;
            }
            var unit =
                Enemies.FirstOrDefault(
                    o => o.IsValidTarget(SpellManager.Q.Range) && o.Health < SpellManager.Q.GetDamage(o));
            if (unit != null && CastQ(unit))
            {
                Console.WriteLine("QKS");
                return true;
            }
            return false;
        }

        public static float GetComboDamage(Obj_AI_Base unit)
        {
            var d = 0d;

            d += Player.GetAutoAttackDamage(unit, true) * 3;

            if (SpellManager.Q.IsReady())
            {
                d += 2 * SpellManager.Q.GetDamage(unit);
            }

            if (SpellManager.W.IsReady())
            {
                d += 3 * SpellManager.W.GetDamage(unit);
            }

            if (SpellManager.E.IsReady())
            {
                d += SpellManager.E.GetDamage(unit);
            }

            if (SpellManager.Ignite != null && SpellManager.Ignite.IsReady())
            {
                d += Player.GetSummonerSpellDamage(unit, Damage.SummonerSpell.Ignite);
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

            return (float) d;
        }
    }
}