using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;
using TreeLib.Extensions;
using TreeLib.Objects;
using TreeLib.SpellData;

namespace EatShitSivir
{
    internal class Sivir : Champion
    {
        private static readonly Dictionary<string, SpellSlot> DangerousSpells = new Dictionary<string, SpellSlot>
        {
            { "darius", SpellSlot.R },
            { "fiddlesticks", SpellSlot.Q },
            { "garen", SpellSlot.R },
            { "leesin", SpellSlot.R },
            { "nautilius", SpellSlot.R },
            { "skarner", SpellSlot.R },
            { "syndra", SpellSlot.R },
            { "warwick", SpellSlot.R },
            { "zed", SpellSlot.R },
            { "tristana", SpellSlot.R }
        };

        public Sivir()
        {
            Q.Range = 1220;
            Q.SetSkillshot(0.25f, 90f, 1350f, false, SkillshotType.SkillshotLine);

            W.Range = 593;

            Menu = new Menu("EatShitSivir", "EatShitSivir", true);

            Orbwalker = Menu.AddOrbwalker();

            Menu.AddSpell(
                SpellSlot.Q,
                new List<Orbwalking.OrbwalkingMode>
                {
                    Orbwalking.OrbwalkingMode.Combo,
                    Orbwalking.OrbwalkingMode.Mixed,
                    Orbwalking.OrbwalkingMode.LaneClear,
                    Orbwalking.OrbwalkingMode.LastHit
                });

            var w = Menu.AddSpell(
                SpellSlot.W,
                new List<Orbwalking.OrbwalkingMode>
                {
                    Orbwalking.OrbwalkingMode.Combo,
                    Orbwalking.OrbwalkingMode.Mixed,
                    Orbwalking.OrbwalkingMode.LaneClear
                });
            w.AddSlider("MinMinionsW", "Min Minions to W", 1, 5);
            w.AddBool("TurretW", "Use on Turret");
            w.AddBool("InhibitorW", "Use on Inhibitor");
            w.AddBool("NexusW", "Use on Nexus");

            var e = Menu.AddMenu("E", "E");
            e.AddBool("AutoE", "Smart E");
            e.AddBool("EatShit", "Eat Shit");
            e.Item("EatShit").SetTooltip("Disable evade scripts in order to cast E and eat a spell.");
            e.AddSlider("EatShitMana", "Min Mana to Eat Shit", 100);

            Menu.AddToMainMenu();
        }

        private static int MinMinionsW
        {
            get { return Menu.Item("MinMinionsW").GetValue<Slider>().Value; }
        }

        private static int EatShitMana
        {
            get { return Menu.Item("EatShitMana").GetValue<Slider>().Value; }
        }

        public override void OnCombo(Orbwalking.OrbwalkingMode mode)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (target == null || !target.IsValid)
            {
                return;
            }

            if (Q.IsReady() && Q.IsActive() && Q.Cast(target).IsCasted()) {}
        }

        public override void OnFarm(Orbwalking.OrbwalkingMode mode)
        {
            if (!Q.IsReady() || !Q.IsActive())
            {
                return;
            }

            var minions = MinionManager.GetMinions(Q.Range);
            var minion = minions.FirstOrDefault(m => m.IsValid && Q.IsKillable(m));
            var farm = Q.GetLineFarmLocation(minions);

            if (minion == null && (mode.Equals(Orbwalking.OrbwalkingMode.LastHit) || Q.Cast(farm.Position)))
            {
                return;
            }

            Q.Cast(minion);
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!W.IsReady() || !W.IsActive())
            {
                return;
            }

            if (target is Obj_AI_Hero && W.Cast())
            {
                return;
            }

            if (ActiveMode.Equals(Orbwalking.OrbwalkingMode.LaneClear) && target is Obj_AI_Minion &&
                MinionManager.GetMinions(AutoAttackRange + 165).Count >= MinMinionsW && W.Cast())
            {
                return;
            }

            if (Menu.Item("TurretW").IsActive() && target is Obj_AI_Turret && W.Cast())
            {
                return;
            }

            if (Menu.Item("InhibitorW").IsActive() && target is Obj_BarracksDampener && W.Cast())
            {
                return;
            }

            if (Menu.Item("NexusW").IsActive() && target is Obj_HQ && W.Cast()) {}
        }

        public override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!Menu.Item("AutoE").IsActive() || !E.IsReady())
            {
                return;
            }

            var hero = sender as Obj_AI_Hero;

            if (hero == null || !hero.IsValid || !hero.IsEnemy)
            {
                return;
            }

            if (Menu.Item("EatShit").IsActive() && SpellDatabase.GetByName(args.SData.Name) != null)
            {
                return;
            }

            SpellSlot spell;

            if (DangerousSpells.TryGetValue(hero.ChampionName.ToLower(), out spell) && args.Slot.Equals(spell))
            {
                E.Cast();
            }
        }

        public override void OnUpdate()
        {
            if (Player.IsDead || !Menu.Item("EatShit").IsActive() || !E.IsReady() || Player.ManaPercent > EatShitMana)
            {
                return;
            }

            if (!WillSkillshotHit(400 + 1000 * (E.Delay + Game.Ping / 2f)))
            {
                return;
            }

            EvadeDisabler.DisableEvade(500);
            E.Cast();
        }

        private static bool WillSkillshotHit(float time)
        {
            return Evade.GetSkillshotsAboutToHit(Player, (int) time).Any();
        }
    }
}