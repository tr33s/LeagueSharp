using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeBlanc.Properties;
using SharpDX;
using Color = System.Drawing.Color;

namespace LeBlanc
{
    internal class Program
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static Spell Q
        {
            get { return Spells.Q; }
        }

        public static Spell E
        {
            get { return Spells.E; }
        }

        public static Spell W
        {
            get { return Spells.W; }
        }

        public static Spell R
        {
            get { return Spells.R; }
        }

        public static void Main(string[] args)
        {
            LeagueSharp.Common.Utils.ClearConsole();
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #region Load

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.IsChampion("Leblanc"))
            {
                return;
            }

            #region Menu

            Menu = new Menu("LeBlanc The Schemer", "LeBlanc", true);

            Orbwalker = Menu.AddOrbwalker();
            Menu.AddTargetSelector();


            var combo = new Combo();
            Menu.AddSubMenu(Combo.LocalMenu);

            var harass = new Harass();
            Menu.AddSubMenu(Harass.LocalMenu);

            var laneclear = new LaneClear();
            Menu.AddSubMenu(LaneClear.LocalMenu);

            var flee = new Flee();
            Menu.AddSubMenu(Flee.LocalMenu);

            var clone = new Clone();
            Menu.AddSubMenu(Clone.LocalMenu);

            var draw = Menu.AddMenu("Draw Settings", "Draw");
            draw.AddItem(new MenuItem("Draw0", "Draw Q Range").SetValue(new Circle(true, Color.Red, Q.Range)));
            draw.AddItem(new MenuItem("Draw1", "Draw W Range").SetValue(new Circle(false, Color.Red, W.Range)));
            draw.AddItem(new MenuItem("Draw2", "Draw E Range").SetValue(new Circle(true, Color.Purple, E.Range)));
            draw.AddBool("DrawCD", "Draw on CD");
            draw.AddBool("DamageIndicator", "Damage Indicator");

            var misc = Menu.AddMenu("Misc Settings", "Misc");

            var ks = new KillSteal();
            misc.AddSubMenu(KillSteal.LocalMenu);

            misc.AddBool("Interrupt", "Interrupt Spells");
            misc.AddBool("AntiGapcloser", "AntiGapCloser");
            misc.AddBool("Sounds", "Sounds");
            misc.AddBool("Troll", "Troll");

            Menu.AddToMainMenu();

            #endregion

            DamageIndicator.DamageToUnit = GetComboDamage;

            if (misc.Item("Sounds").GetValue<bool>())
            {
                var sound = new SoundObject(Resources.OnLoad);
                sound.Play();
            }

            Game.PrintChat(
                "<b><font color =\"#FFFFFF\">LeBlanc the Schemer by </font><font color=\"#0033CC\">Trees</font><font color =\"#FFFFFF\"> loaded!</font></b>");

            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            //Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        #endregion

        #region Events

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var unit = gapcloser.Sender;

            if (!Menu.Item("Interrupt").GetValue<bool>() || !unit.IsValidTarget(E.Range) || !E.IsReady())
            {
                return;
            }

            E.CastIfHitchanceEquals(unit, HitChance.Medium);

            Utility.DelayAction.Add(
                (int) E.Delay * 1000 + (Game.Ping * 1000) / 2 + 50, () =>
                {
                    if (R.IsReady(SpellSlot.E))
                    {
                        R.CastIfHitchanceEquals(SpellSlot.E, unit, HitChance.Medium);
                    }
                });
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("Interrupt").GetValue<bool>() || !sender.IsValidTarget(E.Range) ||
                args.DangerLevel < Interrupter2.DangerLevel.High || !E.IsReady())
            {
                return;
            }

            E.CastIfHitchanceEquals(sender, HitChance.Medium);

            Utility.DelayAction.Add(
                (int) E.Delay * 1000 + (Game.Ping * 1000) / 2 + 50, () =>
                {
                    if (R.IsReady(SpellSlot.E))
                    {
                        R.CastIfHitchanceEquals(SpellSlot.E, sender, HitChance.Medium);
                    }
                });
        }

        #endregion

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            var wBackCircle = Combo.WBackPosition;

            if (wBackCircle != null && wBackCircle.Position != Vector3.Zero)
            {
                Render.Circle.DrawCircle(Combo.WBackPosition.Position, 200, Color.Red, 8);
            }

            foreach (var spell in
                Player.Spellbook.GetMainSpells().Where(s => s.IsReady() || Menu.Item("DrawCD").GetValue<bool>()))
            {
                try
                {
                    var circle = Menu.Item("Draw" + (int) spell.Slot).GetValue<Circle>();
                    if (circle.Active && spell.Level > 0)
                    {
                        Render.Circle.DrawCircle(
                            Player.Position, circle.Radius, spell.IsReady() ? circle.Color : Color.Red);
                    }
                }
                catch {}
            }
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady())
            {
                var d = Player.GetSpellDamage(enemy, SpellSlot.Q);

                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d *= 2;
                }

                damage += d;
            }

            if (R.IsReady())
            {
                var d = 0d;
                var level = Player.Spellbook.GetSpell(SpellSlot.R).Level;
                var maxDamage = new double[] { 200, 400, 600 }[level - 1] + 1.3f * Player.FlatMagicDamageMod;

                switch (R.GetSpellSlot())
                {
                    case SpellSlot.Q:
                        var qDmg = Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            new double[] { 100, 200, 300 }[level - 1] + .65f * Player.FlatMagicDamageMod);
                        d = qDmg > maxDamage ? maxDamage : qDmg;
                        break;
                    case SpellSlot.W:
                        d = Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            new double[] { 150, 300, 450 }[level - 1] + .975f * Player.FlatMagicDamageMod);
                        break;
                    case SpellSlot.E:
                        var eDmg = Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            new double[] { 100, 200, 300 }[level - 1] + .65f * Player.FlatMagicDamageMod);
                        d = eDmg > maxDamage ? maxDamage : eDmg;
                        break;
                }

                if (enemy.HasQBuff() || enemy.HasQRBuff())
                {
                    d += Player.GetSpellDamage(enemy, SpellSlot.Q);
                }

                damage += d;
            }

            if (E.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);
            }

            if (W.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (Items.FQC.IsReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.FrostQueenClaim);
            }

            if (Items.BOTRK.IsReady())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            }

            if (Items.LT.HasItem())
            {
                damage += Player.GetItemDamage(enemy, Damage.DamageItems.LiandrysTorment);
            }

            if (Spells.Ignite.IsReady())
            {
                damage += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            damage += Player.GetAutoAttackDamage(enemy, true);

            return (float) damage;
        }

        #endregion
    }
}