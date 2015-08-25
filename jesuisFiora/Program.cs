using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using jesuisFiora.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace jesuisFiora
{
    internal static class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static Menu Menu;
        public static Color LorahColor = new Color(255, 0, 255);
        public static UltTarget UltTarget;

        public static List<Obj_GeneralParticleEmitter> FioraUltPassiveObjects
        {
            get
            {
                return
                    ObjectManager.Get<Obj_GeneralParticleEmitter>()
                        .Where(
                            obj =>
                                obj.IsValid && obj.Name.Contains("Fiora_Base_R_Mark") ||
                                (obj.Name.Contains("Fiora_Base_R") && obj.Name.Contains("Timeout")))
                        .ToList();
            }
        }

        public static List<Obj_GeneralParticleEmitter> FioraPassiveObjects
        {
            get
            {
                return
                    ObjectManager.Get<Obj_GeneralParticleEmitter>()
                        .Where(obj => obj.IsValid && obj.Name.Contains("Fiora_Base_Passive"))
                        .ToList();
            }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Fiora")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 400 + 175);
            Q.SetSkillshot(.25f, 0, 500, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 750);
            W.SetSkillshot(0.5f, 95, 3000, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 500);
            R.SetTargetted(.066f, 500);

            Menu = new Menu("jesuisFiora", "jesuisFiora", true);
            Menu.SetFontStyle(FontStyle.Regular, LorahColor);
            Orbwalker = Menu.AddOrbwalker();

            Menu.AddTargetSelector();

            var spells = Menu.AddMenu("Spells", "Spells");

            var qMenu = spells.AddMenu("Q", "Q");
            qMenu.AddBool("QCombo", "Use in Combo");
            qMenu.AddBool("QHarass", "Use in Harass");
            qMenu.AddInfo("QFleeInfo", "Flee:", LorahColor);
            qMenu.AddKeyBind("QFlee", "Q Flee", 'T');
            qMenu.AddInfo("FleeInfo", " --> Flees towards cursor position.", LorahColor);
            qMenu.AddBool("QKillsteal", "Use for Killsteal");

            var wMenu = spells.AddMenu("W", "W");
            var wSpells = wMenu.AddMenu("BlockSpells", "Blocked Spells");
            wMenu.AddBool("WSpells", "W Incoming Spells");
            wMenu.AddBool("WKillsteal", "Use for Killsteal");
            wMenu.AddBool("WTurret", "Block W Under Enemy Turret");

            SpellBlock.Initialize(wSpells);

            var eMenu = spells.AddMenu("E", "E");
            eMenu.AddBool("ECombo", "Use in Combo");
            eMenu.AddBool("EHarass", "Use in Harass");

            var rMenu = spells.AddMenu("R", "R");
            rMenu.AddBool("RCombo", "Use R");
            rMenu.AddList("RMode", "Cast Mode", new[] { "Duelist", "Combo" });
            rMenu.AddKeyBind("RToggle", "Toggle Mode", 'L');

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

            rMenu.AddInfo("RModeInfo", " --> Duelist Mode: Only use R when target is killable.", LorahColor);
            rMenu.AddInfo("RModeInfo2", " --> Combo Mode: Use R in normal combo", LorahColor);
            rMenu.AddSlider("RKillVital", "Duelist Mode Min Vitals", 1, 0, 4);
            rMenu.AddInfo("RVitalInfo", " --> Note: This is only for damage calculation in Duelist Mode.", LorahColor);
            rMenu.AddBool("RComboSelected", "Use R Selected Only");

            var items = spells.AddMenu("Items", "Items");
            items.AddBool("ItemsCombo", "Use in Combo");
            items.AddBool("ItemsHarass", "Use in Harass");

            spells.AddSlider("ManaHarass", "Harass Min Mana Percent", 40);

            var farm = Menu.AddMenu("Farm", "Farm");

            var qFarm = farm.AddMenu("Farm", "Q");
            qFarm.AddBool("QLastHit", "Q Last Hit (Only Killable)");
            qFarm.AddBool("QLaneClear", "Q LaneClear (All)");
            qFarm.AddSlider("QFarmMana", "Q Min Mana Percent", 40);

            var eFarm = farm.AddMenu("E", "E");
            eFarm.AddBool("ELaneClear", "Use in LaneClear");

            farm.AddKeyBind("FarmEnabled", "Farm Enabled in LC & LH", 'J', KeyBindType.Toggle, true);
            farm.AddInfo("FarmInfo", " --> LC = LaneClear , LH = LastHit", LorahColor);
            farm.AddBool("ItemsLaneClear", "Use Items in LaneClear");


            var draw = Menu.AddMenu("Drawing", "Drawing");
            draw.AddBool("QDraw", "Draw Q");
            draw.AddBool("WDraw", "Draw W");
            draw.AddBool("RDraw", "Draw R");
            draw.AddBool("DuelistDraw", "Duelist Mode: Killable Target");
            draw.AddBool("FarmPermashow", "Permashow Farm Enabled");
            draw.AddBool("RPermashow", "Permashow R Mode");

            if (draw.Item("RPermashow").IsActive())
            {
                rMenu.Item("RMode").Permashow(true, null, LorahColor);
            }

            draw.Item("RPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    rMenu.Item("RMode").Permashow(eventArgs.GetNewValue<bool>(), null, LorahColor);
                };

            if (draw.Item("FarmPermashow").IsActive())
            {
                farm.Item("FarmEnabled").Permashow(true, null, LorahColor);
            }

            draw.Item("FarmPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    farm.Item("FarmEnabled").Permashow(eventArgs.GetNewValue<bool>(), null, LorahColor);
                };

            var dmg = draw.AddMenu("DamageIndicator", "Damage Indicator");
            dmg.AddBool("DmgEnabled", "Draw Damage Indicator");
            dmg.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
            dmg.AddCircle("FillColor", "Damage Color", System.Drawing.Color.HotPink);
            dmg.AddBool("Killable", "Killable Text");

            Menu.AddBool("Sounds", "Sounds");
            Menu.AddInfo("Info", "By Trees and lorah!", LorahColor);
            Menu.AddToMainMenu();

            if (Menu.Item("Sounds").IsActive())
            {
                new SoundObject(Resources.OnLoad).Play();
            }

            DamageIndicator.DamageToUnit = GetComboDamage;

            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.PrintChat(
                "<font color=\"{0}\">jesuisFiora Loaded!</font>", System.Drawing.Color.DeepPink.ToHexString());
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            Flee();
            DuelistMode();
            KillstealW();

            var mode = Orbwalker.ActiveMode;
            var combo = mode.Equals(Orbwalking.OrbwalkingMode.Combo) || mode.Equals(Orbwalking.OrbwalkingMode.Mixed);


            if (combo)
            {
                var comboMode = mode.GetModeString();
                var target = UltTarget != null && UltTarget.Target.IsValidTarget(Q.Ran)
                    ? UltTarget.Target
                    : TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);

                if (target == null || !target.IsValidTarget(W.Range))
                {
                    return;
                }

                var qCombo = Menu.Item("Q" + comboMode).IsActive();
                var rCombo = comboMode.Equals("Combo") && Menu.Item("R" + comboMode).IsActive() &&
                             Menu.Item("RMode").GetValue<StringList>().SelectedIndex.Equals(1);

                if (comboMode.Equals("Harass") && Player.ManaPercent < Menu.Item("ManaHarass").GetValue<Slider>().Value)
                {
                    return;
                }

                if (qCombo && CastQ(target))
                {
                    return;
                }

                if (Player.IsDashing())
                {
                    return;
                }

                if (rCombo)
                {
                    if (Menu.Item("RComboSelected").IsActive())
                    {
                        var unit = TargetSelector.GetSelectedTarget();
                        if (unit != null && unit.IsValid && unit.NetworkId.Equals(target.NetworkId) && CastR(target))
                        {
                            return;
                        }
                        return;
                    }

                    if (CastR(target))
                    {
                        Hud.SelectedUnit = target;
                    }
                }
            }
            else if (Menu.Item("FarmEnabled").IsActive())
            {
                if (mode.Equals(Orbwalking.OrbwalkingMode.LastHit) && Menu.Item("QLastHit").IsActive() &&
                    Player.ManaPercent >= Menu.Item("QFarmMana").GetValue<Slider>().Value)
                {
                    var killableMinion =
                        MinionManager.GetMinions(Q.Range)
                            .FirstOrDefault(obj => obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));
                    if (killableMinion != null)
                    {
                        Q.Cast(killableMinion);
                    }
                }

                if (mode.Equals(Orbwalking.OrbwalkingMode.LaneClear) && Menu.Item("QLaneClear").IsActive() &&
                    Player.ManaPercent >= Menu.Item("QFarmMana").GetValue<Slider>().Value)
                {
                    var minion = MinionManager.GetMinions(Q.Range).OrderBy(obj => obj.Distance(Player)).FirstOrDefault();
                    if (minion != null)
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsValid)
            {
                return;
            }

            if (sender.IsMe)
            {
                var slot = Player.GetSpellSlot(args.SData.Name);

                if (slot.Equals(SpellSlot.E))
                {
                    Orbwalking.ResetAutoAttackTimer();
                }
                else if (slot.Equals(SpellSlot.R) && args.Target != null)
                {
                    UltTarget = new UltTarget(args.Target);
                }
                return;
            }

            var autoW = Menu.Item("WSpells").IsActive() && W.IsReady();

            if (!autoW)
            {
                return;
            }

            var unit = sender as Obj_AI_Hero;
            var blockableSpell = unit != null && unit.IsEnemy && SpellBlock.Contains(unit, args);
            var type = args.SData.TargettingType;

            if (!blockableSpell)
            {
                return;
            }

            if (type.IsTargeted() && args.Target != null && args.Target.IsMe)
            {
                if (Menu.Item("WTurret").IsActive() && Player.UnderTurret(true))
                {
                    return;
                }

                CastW(sender);
            }
            else if ((type.Equals(SpellDataTargetType.LocationAoe)) &&
                     args.End.Distance(Player.ServerPosition) < args.SData.CastRadius)
            {
                CastW(sender);
            }
            else if (args.SData.IsAutoAttack() && args.Target != null && args.Target.IsMe)
            {
                CastW(sender);
            }
            else if (type.Equals(SpellDataTargetType.SelfAoe) &&
                     unit.Distance(Player.ServerPosition) < args.SData.CastRange + args.SData.CastRadius / 2)
            {
                CastW(sender);
            }
            else if (type.Equals(SpellDataTargetType.Self))
            {
                if ((unit.ChampionName.Equals("Kalista") && Player.Distance(unit) < 350))
                {
                    CastW(sender);
                }
                if (unit.ChampionName.Equals("Zed") && Player.Distance(unit) < 300)
                {
                    Utility.DelayAction.Add(200, () => CastW(sender));
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Menu.Item("QDraw").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Purple, 7);
            }

            if (Menu.Item("WDraw").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.DeepPink, 3);
            }

            if (Menu.Item("RDraw").IsActive())
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, System.Drawing.Color.White, 7);
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            var mode = Orbwalker.ActiveMode;

            if (mode.Equals(Orbwalking.OrbwalkingMode.None) || mode.Equals(Orbwalking.OrbwalkingMode.LastHit))
            {
                return;
            }

            var comboMode = mode.GetModeString();

            if (comboMode.Equals("LaneClear") && !Menu.Item("FarmEnabled").IsActive())
            {
                return;
            }

            if (Menu.Item("E" + comboMode).IsActive() && E.IsReady())
            {
                E.Cast();
            }

            if (Menu.Item("Items" + comboMode).IsActive())
            {
                Utility.DelayAction.Add(
                    (int) (E.Delay * 1000f + Game.Ping / 2f + 20), () => CastItems(TargetSelector.GetSelectedTarget()));
            }
        }

        public static void DuelistMode()
        {
            if (!Menu.Item("RCombo").IsActive() || !Menu.Item("RMode").GetValue<StringList>().SelectedIndex.Equals(0) ||
                !R.IsReady() || Player.CountEnemiesInRange(R.Range) == 0)

            {
                return;
            }

            var vitalCalc = Menu.Item("RKillVital").GetValue<Slider>().Value;
            foreach (var obj in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        enemy =>
                            enemy.IsValidTarget() && GetComboDamage(enemy, vitalCalc) >= enemy.Health &&
                            enemy.Health > Player.GetSpellDamage(enemy, SpellSlot.Q) + GetPassiveDamage(enemy, 1)))
            {
                if (Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Combo) && obj.IsValidTarget(R.Range))
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
                }

                if (Menu.Item("DuelistDraw").IsActive())
                {
                    var pos = obj.HPBarPosition;
                    Drawing.DrawText(pos.X, pos.Y - 30, System.Drawing.Color.DeepPink, "Killable!");
                }
            }
        }

        public static bool CastQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || !target.IsValidTarget(Q.Range))
            {
                return false;
            }

            if (CountPassive(target) == 0)
            {
                return Q.Cast(target).IsCasted();
            }

            var pos = GetPassivePosition(target);

            return pos.Equals(Vector3.Zero) ? Q.Cast(target).IsCasted() : Q.Cast(pos);
        }

        public static void CastW(Obj_AI_Base target)
        {
            if (!target.IsValidTarget(W.Range))
            {
                W.Cast(target.ServerPosition);
            }
            else
            {
                W.Cast(target);
            }
        }

        public static void KillstealQ()
        {
            if (!Menu.Item("QKillsteal").IsActive())
            {
                return;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        o =>
                            o.IsValidTarget(Q.Range) &&
                            o.Health < Q.GetDamage(o) + (CountPassive(o) > 0 ? GetPassiveDamage(o) : 0));
            if (unit != null)
            {
                CastQ(unit);
            }
        }

        public static void KillstealW()
        {
            if (!Menu.Item("WKillsteal").IsActive())
            {
                return;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(o => o.IsValidTarget(W.Range) && o.Health < W.GetDamage(o));
            if (unit != null)
            {
                W.Cast(unit);
            }
        }

        public static bool CastR(Obj_AI_Base target)
        {
            return R.IsReady() && target.IsValidTarget(R.Range) && R.Cast(target).IsCasted();
        }

        public static bool CastItems(Obj_AI_Base target)
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            if (botrk != null && botrk.IsReady() && target.IsValidTarget(botrk.Range))
            {
                botrk.Cast(target);
            }

            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            if (tiamat != null && tiamat.IsReady() && tiamat.Cast())
            {
                return true;
            }

            var hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            if (hydra != null && hydra.IsReady() && hydra.Cast())
            {
                return true;
            }

            const ItemId titanic = (ItemId) 3748;
            var slot = Player.InventoryItems.FirstOrDefault(i => i.Id.Equals(titanic));

            if (slot == null)
            {
                return false;
            }

            var spell = Player.Spellbook.GetSpell(slot.SpellSlot);
            return spell.IsReady() && Player.Spellbook.CastSpell(spell.Slot);
        }

        public static void Flee()
        {
            if (!Menu.Item("QFlee").IsActive())
            {
                return;
            }

            Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            if (Q.IsReady())
            {
                Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, Q.Range + 10));
            }
        }

        public static double GetPassiveDamage(Obj_AI_Base target)
        {
            var count = CountPassive(target);
            return count == 0 ? 0 : GetPassiveDamage(target, count);
        }

        public static double GetPassiveDamage(Obj_AI_Base target, int passiveCount)
        {
            return passiveCount *
                   (.03f +
                    (Math.Min(
                        Math.Max(.028f, (.027 + .001f * Player.Level * Player.FlatPhysicalDamageMod / 100f)), .45f))) *
                   target.MaxHealth;
        }

        public static int CountPassive(Obj_AI_Base target)
        {
            return FioraPassiveObjects.Count(obj => obj.Position.Distance(target.ServerPosition) <= 50) +
                   FioraUltPassiveObjects.Count(obj => obj.Position.Distance(target.ServerPosition) <= 50);
        }

        public static Vector3 GetPassivePosition(Obj_AI_Base target)
        {
            var passive =
                FioraUltPassiveObjects.OrderBy(obj => obj.Position.Distance(target.ServerPosition))
                    .FirstOrDefault(obj => obj.IsValid && obj.IsVisible);
            var d = 320;

            if (passive == null)
            {
                passive = FioraPassiveObjects.FirstOrDefault(obj => obj.Position.Distance(target.ServerPosition) < 50);
                d = 200;
            }

            if (passive == null)
            {
                return Vector3.Zero;
            }

            var pos = Prediction.GetPrediction(target, Q.Delay).UnitPosition.To2D();

            if (passive.Name.Contains("NE"))
            {
                pos.Y += d;
            }

            if (passive.Name.Contains("SE"))
            {
                pos.X -= d;
            }

            if (passive.Name.Contains("NW"))
            {
                pos.X += d;
            }

            if (passive.Name.Contains("SW"))
            {
                pos.Y -= d;
            }

            return pos.To3D();
        }

        public static float GetComboDamage(Obj_AI_Hero unit)
        {
            return GetComboDamage(unit, 0);
        }

        public static float GetComboDamage(Obj_AI_Hero unit, int maxStacks)
        {
            var d = 2 * Player.GetAutoAttackDamage(unit);

            var hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            if (hydra != null && hydra.IsReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Hydra);
            }

            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            if (tiamat != null && tiamat.IsReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Tiamat);
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
                    d += GetPassiveDamage(unit, 4);
                }
                else
                {
                    d += GetPassiveDamage(unit);
                }
            }
            else
            {
                d += GetPassiveDamage(unit, maxStacks);
            }

            return (float) d;
        }
    }

    public class UltTarget
    {
        public int CastTime;
        public int EndTime;
        public Obj_AI_Hero Target;

        public UltTarget(GameObject obj)
        {
            Target = obj as Obj_AI_Hero;
            CastTime = Environment.TickCount;
            EndTime = CastTime + 8000;
            Game.OnUpdate += Game_OnUpdate;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Target == null || Target.IsDead || Environment.TickCount - EndTime > 0)
            {
                Program.UltTarget = null;
            }
        }
    }
}