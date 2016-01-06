using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace TreeLib.Objects
{
    public class Champion
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static Menu Menu;

        public Champion()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnWndProc += Game_OnWndProc;
            Game.OnUpdate += Game_OnUpdate;
        }

        public static Orbwalking.OrbwalkingMode ActiveMode
        {
            get { return Orbwalker.ActiveMode; }
        }

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

        public virtual void Game_OnWndProc(WndEventArgs args) {}

        public virtual void GameObject_OnDelete(GameObject sender, EventArgs args) {}

        public virtual void GameObject_OnCreate(GameObject sender, EventArgs args) {}

        public virtual void Drawing_OnDraw(EventArgs args) {}

        public virtual void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args) {}

        public virtual void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target) {}

        public virtual void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args) {}

        private void Game_OnUpdate(EventArgs args)
        {
            OnUpdate();

            if (Player.IsDead)
            {
                return;
            }

            if (Player.IsDashing() || Player.IsChannelingImportantSpell()) /*|| Player.Spellbook.IsCastingSpell || 
                Player.Spellbook.IsAutoAttacking|| Player.IsWindingUp)*/
            {
                return;
            }

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnCombo(Orbwalker.ActiveMode);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                case Orbwalking.OrbwalkingMode.LastHit:
                    OnFarm(Orbwalker.ActiveMode);
                    break;
            }
        }

        public virtual void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser) {}

        public virtual void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args) {}

        public virtual void OnUpdate() {}
        public virtual void OnCombo(Orbwalking.OrbwalkingMode mode) {}
        public virtual void OnFarm(Orbwalking.OrbwalkingMode mode) {}
    }
}