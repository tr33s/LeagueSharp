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
        public static Color ScriptColor = new Color(255, 0, 255);
        public static Spell Ignite;

        private static IEnumerable<Obj_AI_Hero> Enemies
        {
            get { return HeroManager.Enemies; }
        }

        private static List<Obj_AI_Base> QLaneMinions
        {
            get { return MinionManager.GetMinions(Q.Range); }
        }

        private static List<Obj_AI_Base> QJungleMinions
        {
            get { return MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral); }
        }

        private static IEnumerable<Obj_GeneralParticleEmitter> FioraUltPassiveObjects
        {
            get
            {
                return
                    ObjectManager.Get<Obj_GeneralParticleEmitter>()
                        .Where(
                            obj =>
                                obj.IsValid && obj.Name.Contains("Fiora_Base_R_Mark") ||
                                (obj.Name.Contains("Fiora_Base_R") && obj.Name.Contains("Timeout")));
            }
        }

        private static IEnumerable<Obj_GeneralParticleEmitter> FioraPassiveObjects
        {
            get
            {
                return
                    ObjectManager.Get<Obj_GeneralParticleEmitter>()
                        .Where(obj => obj.IsValid && obj.Name.Contains("Fiora_Base_Passive"));
            }
        }

        private static float FioraAutoAttackRange
        {
            get { return Orbwalking.GetRealAutoAttackRange(Player); }
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

            Q = new Spell(SpellSlot.Q, 400 + 350);
            Q.SetSkillshot(.25f, 0, 500, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 750);
            W.SetSkillshot(0.5f, 70, 3200, false, SkillshotType.SkillshotLine);

            E = new Spell(SpellSlot.E);

            R = new Spell(SpellSlot.R, 500);
            R.SetTargetted(.066f, 500);

            Menu = new Menu("jesuisFiora", "jesuisFiora", true);
            Menu.SetFontStyle(FontStyle.Regular, ScriptColor);

            Orbwalker = Menu.AddOrbwalker();
            Menu.AddTargetSelector();

            var spells = Menu.AddMenu("Spells", "Spells");

            var qMenu = spells.AddMenu("Q", "Q");
            qMenu.AddBool("QCombo", "Use in Combo");
            qMenu.AddBool("QHarass", "Use in Harass");
            qMenu.AddSlider("QRange", "Decrease Q Max Range", 10, 0, 150);
            Q.Range = 750 - qMenu.Item("QRange").GetValue<Slider>().Value;
            qMenu.Item("QRange").ValueChanged += (sender, eventArgs) =>
            {
                Q.Range = 750 - eventArgs.GetNewValue<Slider>().Value;
                var qDraw = Menu.Item("QDraw");
                var qCircle = qDraw.GetValue<Circle>();
                qDraw.SetValue(new Circle(qCircle.Active, qCircle.Color, Q.Range));
            };
            qMenu.AddBool("QForcePassive", "Only Q to Vitals", false);
            qMenu.AddInfo("QFleeInfo", "Flee:", ScriptColor);
            qMenu.AddKeyBind("QFlee", "Q Flee", 'T');
            qMenu.AddInfo("FleeInfo", " --> Flees towards cursor position.", ScriptColor);
            qMenu.AddBool("QKillsteal", "Use for Killsteal");

            var wMenu = spells.AddMenu("W", "W");
            var wSpells = wMenu.AddMenu("BlockSpells", "Blocked Spells");
            wMenu.AddKeyBind("WSpells", "W Spellblock", 'U', KeyBindType.Toggle, true);
            wMenu.AddList("WMode", "W Spellblock to: ", new[] { "Spell Caster", "Target" });
            wMenu.AddBool("WKillsteal", "Use for Killsteal");
            wMenu.AddBool("WTurret", "Block W Under Enemy Turret");

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

            rMenu.AddInfo("RModeInfo", " --> Duelist Mode: Only use R when target is killable.", ScriptColor);
            rMenu.AddInfo("RModeInfo2", " --> Combo Mode: Use R in normal combo", ScriptColor);
            rMenu.AddSlider("RKillVital", "Duelist Mode Min Vitals", 1, 0, 4);
            rMenu.AddInfo("RVitalInfo", " --> Note: This is only for damage calculation in Duelist Mode.", ScriptColor);
            rMenu.AddBool("RComboSelected", "Use R Selected on Selected Unit Only");

            var items = spells.AddMenu("Items", "Items");
            items.AddBool("ItemsCombo", "Use in Combo");
            items.AddBool("ItemsHarass", "Use in Harass");

            spells.AddBool("Ignite", "Auto Ignite");
            spells.AddSlider("ManaHarass", "Harass Min Mana Percent", 40);

            var farm = Menu.AddMenu("Farm", "Farm");

            var qFarm = farm.AddMenu("Farm", "Q");
            qFarm.AddBool("QLastHit", "Q Last Hit (Only Killable)");
            qFarm.AddBool("QLaneClear", "Q LaneClear (All)");
            qFarm.AddBool("QFarmAA", "Only Q out of AA Range", false);
            qFarm.AddSlider("QFarmMana", "Q Min Mana Percent", 40);

            var eFarm = farm.AddMenu("E", "E");
            eFarm.AddBool("ELaneClear", "Use in LaneClear");

            farm.AddKeyBind("FarmEnabled", "Farm Enabled", 'J', KeyBindType.Toggle, true);
            farm.AddInfo("FarmInfo", " --> Enabled in LaneClear and LastHit", ScriptColor);
            farm.AddBool("ItemsLaneClear", "Use Items in LaneClear");

            var draw = Menu.AddMenu("Drawing", "Drawing");
            draw.AddCircle("QDraw", "Draw Q", System.Drawing.Color.Purple, Q.Range);
            draw.AddCircle("WDraw", "Draw W", System.Drawing.Color.DeepPink, W.Range);
            draw.AddCircle("RDraw", "Draw R", System.Drawing.Color.White, R.Range);
            draw.AddBool("DuelistDraw", "Duelist Mode: Killable Target");
            draw.AddBool("WPermashow", "Permashow W Spellblock");
            draw.AddBool("RPermashow", "Permashow R Mode");
            draw.AddBool("FarmPermashow", "Permashow Farm Enabled");

            if (draw.Item("WPermashow").IsActive())
            {
                wMenu.Item("WSpells").Permashow(true, null, ScriptColor);
            }

            draw.Item("WPermashow").ValueChanged +=
                (sender, eventArgs) =>
                {
                    wMenu.Item("WSpells").Permashow(eventArgs.GetNewValue<bool>(), null, ScriptColor);
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

            var dmg = draw.AddMenu("DamageIndicator", "Damage Indicator");
            dmg.AddBool("DmgEnabled", "Draw Damage Indicator");
            dmg.AddCircle("HPColor", "Predicted Health Color", System.Drawing.Color.White);
            dmg.AddCircle("FillColor", "Damage Color", System.Drawing.Color.HotPink);
            dmg.AddBool("Killable", "Killable Text");

            Menu.AddBool("Sounds", "Sounds");
            Menu.AddInfo("Info", "By Trees!", ScriptColor);
            Menu.AddToMainMenu();

            if (Menu.Item("Sounds").IsActive())
            {
                new SoundObject(Resources.OnLoad).Play();
            }

            var ignite = ObjectManager.Player.Spellbook.GetSpell(ObjectManager.Player.GetSpellSlot("summonerdot"));
            if (ignite.Slot != SpellSlot.Unknown)
            {
                Ignite = new Spell(ignite.Slot, 600);
                //Ignite.SetTargetted();
            }

            DamageIndicator.DamageToUnit = GetComboDamage;

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
            if (Player.IsDead || Flee())
            {
                return;
            }

            DuelistMode();
            Farm();
            KillstealQ();
            KillstealW();
            AutoIgnite();

            var mode = Orbwalker.ActiveMode;
            var combo = mode.Equals(Orbwalking.OrbwalkingMode.Combo) || mode.Equals(Orbwalking.OrbwalkingMode.Mixed);


            if (!combo)
            {
                return;
            }

            var comboMode = mode.GetModeString();
            var target = UltTarget.Target != null && UltTarget.Target.IsValidTarget(Q.Range)
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

            if (Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            if (rCombo && ComboR(target))
            {
                return;
            }

            if (qCombo)
            {
                if (target.IsValidTarget(FioraAutoAttackRange) && !Orbwalking.IsAutoAttack(Player.LastCastedSpellName()))
                {
                    return;
                }

                var path = target.GetWaypoints();
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

                return;
            }

            var autoW = Menu.Item("WSpells").IsActive() && W.IsReady();

            if (!autoW)
            {
                return;
            }

            var unit = sender as Obj_AI_Hero;
            var castUnit = unit;
            var type = args.SData.TargettingType;

            var blockableSpell = unit != null && unit.IsEnemy && SpellBlock.Contains(unit, args);
            if (!blockableSpell)
            {
                //Console.WriteLine("RETURN");
                return;
            }

            //Console.WriteLine(type);
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

            if (type.IsTargeted() && args.Target != null && args.Target.IsMe)
            {
                if (Menu.Item("WTurret").IsActive() && Player.UnderTurret(true))
                {
                    return;
                }

                CastW(castUnit);
            }
            else if (unit.ChampionName.Equals("Riven") && unit.Distance(Player) < 400)
            {
                CastW(castUnit);
            }
            else if (unit.ChampionName.Equals("Bard") && type.Equals(SpellDataTargetType.Location) &&
                     args.End.Distance(Player.ServerPosition) < 300)
            {
                Utility.DelayAction.Add(400 + (int) (unit.Distance(Player) / 7f), () => CastW(castUnit));
            }
            else if (args.SData.IsAutoAttack() && args.Target != null && args.Target.IsMe)
            {
                CastW(castUnit);
            }
            else if (type.Equals(SpellDataTargetType.SelfAoe) &&
                     unit.Distance(Player.ServerPosition) < args.SData.CastRange + args.SData.CastRadius / 2)
            {
                CastW(castUnit);
            }
            else if (type.Equals(SpellDataTargetType.Self))
            {
                // this probably isn't needed
                if ((unit.ChampionName.Equals("Kalista") && Player.Distance(unit) < 350))
                {
                    CastW(castUnit);
                }

                // need to look into this
                if (unit.ChampionName.Equals("Zed") && Player.Distance(unit) < 300)
                {
                    Utility.DelayAction.Add(200, () => CastW(castUnit));
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            foreach (var circle in
                new[] { "Q", "W", "R" }.Select(spell => Menu.Item(spell + "Draw").GetValue<Circle>())
                    .Where(circle => circle.Active))
            {
                Render.Circle.DrawCircle(Player.Position, circle.Radius, circle.Color);
            }
        }

        private static void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var targ = args.Target as Obj_AI_Base;
            if (!args.Unit.IsMe || targ == null)
            {
                return;
            }

            if (!Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            var mode = Orbwalker.ActiveMode.GetModeString();
            if (!Menu.Item("E" + mode).IsActive() || !E.IsReady())
            {
                return;
            }

            if (!targ.IsFacing(Player) && targ.Distance(Player) >= FioraAutoAttackRange - 10)
            {
                E.Cast();
            }
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var targ = target as Obj_AI_Base;
            if (!unit.IsMe || targ == null)
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

            if (Menu.Item("E" + comboMode).IsActive() && E.IsReady() && E.Cast())
            {
                return;
            }

            if (Menu.Item("Items" + comboMode).IsActive())
            {
                CastItems(targ);
            }
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
                HeroManager.Enemies.Where(
                    enemy =>
                        Menu.Item("Duelist" + enemy.ChampionName).IsActive() && enemy.IsValidTarget(R.Range) &&
                        GetComboDamage(enemy, vitalCalc) >= enemy.Health &&
                        enemy.Health > Player.GetSpellDamage(enemy, SpellSlot.Q) + GetPassiveDamage(enemy, 1)))
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
            if (!Menu.Item("FarmEnabled").IsActive() ||
                !mode.Equals(Orbwalking.OrbwalkingMode.LaneClear) && !mode.Equals(Orbwalking.OrbwalkingMode.LastHit))
            {
                return;
            }

            var active = Q.IsReady() && Menu.Item("Q" + mode.GetModeString()).IsActive() &&
                         Player.ManaPercent >= Menu.Item("QFarmMana").GetValue<Slider>().Value;

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
            if (jungle != null && Q.Cast(jungle).IsCasted())
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
            if (lane != null && Q.Cast(lane).IsCasted()) {}
        }

        public static bool CastQ(Obj_AI_Base target)
        {
            if (!Q.IsReady() || !target.IsValidTarget(Q.Range))
            {
                return false;
            }


            var forcePassive = Menu.Item("QForcePassive").IsActive();
            var predicted = Prediction.GetPrediction(target, Q.Delay);
            var condition = predicted.Hitchance > HitChance.High && Q.IsInRange(predicted.UnitPosition);

            if (CountPassive(target) == 0)
            {
                if (forcePassive)
                {
                    Console.WriteLine("NO PASSIVE");
                    return false;
                }

                return condition && Q.Cast(predicted.UnitPosition);
            }

            var pos = GetPassivePosition(target);
            if (pos.Equals(Vector3.Zero))
            {
                if (forcePassive)
                {
                    Console.WriteLine("CAN'T FIND PASSIVE POS");
                    return false;
                }
                return condition && Q.Cast(predicted.UnitPosition);
            }

            if (Player.Distance(pos) > Q.Range)
            {
                if (forcePassive)
                {
                    Console.WriteLine("PASSIVE OUT OF RANGE");
                    return false;
                }
                return condition && Q.Cast(predicted.UnitPosition);
            }
            return Q.Cast(pos);
        }

        public static bool CastW(Obj_AI_Base target)
        {
            return target.IsValidTarget(W.Range) ? W.Cast(target).IsCasted() : W.Cast(target.ServerPosition);
        }

        public static void KillstealQ()
        {
            if (!Menu.Item("QKillsteal").IsActive())
            {
                return;
            }

            var unit =
                Enemies.FirstOrDefault(o => o.IsValidTarget(Q.Range) && o.Health < Q.GetDamage(o) + GetPassiveDamage(o));
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

        public static bool CastR(Obj_AI_Base target)
        {
            return R.IsReady() && target.IsValidTarget(R.Range) && R.Cast(target).IsCasted();
        }

        public static bool CastItems(Obj_AI_Base target)
        {
            if (Player.IsDashing() || Player.IsWindingUp)
            {
                return false;
            }

            var youmuus = ItemData.Youmuus_Ghostblade.GetItem();
            if (youmuus != null && youmuus.IsReady() && youmuus.Cast())
            {
                return true;
            }

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            if (botrk != null && botrk.IsReady() && target.IsValidTarget(botrk.Range))
            {
                botrk.Cast(target);
            }

            var units =
                MinionManager.GetMinions(385, MinionTypes.All, MinionTeam.NotAlly).Count(o => !(o is Obj_AI_Turret));
            var heroes = Player.GetEnemiesInRange(385).Count;
            var count = units + heroes;

            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            if (tiamat != null && tiamat.IsReady() && count >= 1 && tiamat.Cast())
            {
                return true;
            }

            var hydra = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            if (hydra != null && hydra.IsReady() && count >= 1 && hydra.Cast())
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
            return spell.IsReady() && count >= 1 && Player.Spellbook.CastSpell(spell.Slot);
        }

        public static void AutoIgnite()
        {
            if (!Menu.Item("Ignite").IsActive() || Ignite == null || !Ignite.IsReady())
            {
                return;
            }

            var target =
                HeroManager.Enemies.FirstOrDefault(
                    h =>
                        h.IsValidTarget(Ignite.Range) &&
                        h.Health < Player.GetSummonerSpellDamage(h, Damage.SummonerSpell.Ignite));
            if (target != null)
            {
                Ignite.Cast(target);
            }
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
            Console.WriteLine("SEARCH PASSIVE " + target.Name);
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

            const ItemId titanic = (ItemId) 3748;
            var slot = Player.InventoryItems.FirstOrDefault(i => i.Id.Equals(titanic));

            if (slot != null && Player.Spellbook.GetSpell(slot.SpellSlot).IsReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Hydra);
            }

            var tiamat = ItemData.Tiamat_Melee_Only.GetItem();
            if (tiamat != null && tiamat.IsReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Tiamat);
            }

            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            if (cutlass != null && cutlass.IsReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Bilgewater);
            }

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            if (botrk != null && botrk.IsReady())
            {
                d += Player.GetItemDamage(unit, Damage.DamageItems.Botrk);
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
}