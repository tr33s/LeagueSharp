using System;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Orbwalking;
using SharpDX;

namespace LeBlanc
{
    internal class Combo
    {
        private const string Name = "Combo";
        public static Menu LocalMenu;
        public static WPosition WBackPosition;
        public static readonly string LeBlancWObject = "LeBlanc_Base_W_return_indicator.troy";
        private static Obj_AI_Hero CurrentTarget;

        static Combo()
        {
            #region Menu

            var combo = new Menu(Name + " Settings", Name);

            var comboQ = combo.AddMenu("Q", "Q");
            comboQ.AddBool("ComboQ", "Use Q");

            var comboW = combo.AddMenu("W", "W");
            comboW.AddBool("ComboW", "Use W");
            comboW.AddObject("Spacer", "Set to 0% To Always W");
            comboW.AddSlider("ComboWMinHP", "Min HP To Use W", 20);
            comboW.AddBool("ComboW2", "Use Second W");
            comboW.AddBool("ComboW2Spells", "Use After Spells on CD");

            var comboE = combo.AddMenu("E", "E");
            comboE.AddBool("ComboE", "Use E");
            comboE.AddBool("ComboEStart", "Start Combo with E", false);
            comboE.AddHitChance("ComboEHC", "Min HitChance", HitChance.Medium);

            var comboR = combo.AddMenu("R", "R");
            comboR.AddBool("ComboR", "Use R");
            comboR.AddList(
                "ComboRMode", "Ult Mode",
                new[] { SpellSlot.Q.ToString(), SpellSlot.W.ToString(), SpellSlot.E.ToString() });

            /*  var wCombo = combo.AddMenu("AOE Combo", "AOECombo");
            wCombo.AddBool("wComboEnabled", "Enabled");
            wCombo.AddBool("wComboFlash", "Use Flash");
            wCombo.AddSlider("wComboEnemies", "Min Enemies", 2, 1, 5);
            */
            combo.AddBool("ComboItems", "Use Items");
            combo.AddKeyBind("ComboKey", "Combo Key", 32);

            #endregion

            LocalMenu = combo;

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Menu.Item(Name + "Key").GetValue<KeyBind>().Active; }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Spell Q
        {
            get { return Spells.Q; }
        }

        private static Spell W
        {
            get { return Spells.W; }
        }

        private static Spell E
        {
            get { return Spells.E; }
        }

        private static Spell R
        {
            get { return Spells.R; }
        }

        private static HitChance EHitChance
        {
            get { return Menu.Item("ComboEHC").GetHitChance(); }
        }

        private static SpellDataInst Flash
        {
            get { return Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerflash")); }
        }

        private static void ComboLogic()
        {
            var d = Player.Distance(CurrentTarget);
            var eFirst = Menu.Item("ComboEStart").GetValue<bool>();
            var castRE = R.IsReady(SpellSlot.E) && GetMenuUlt() == SpellSlot.E;
            var qFirst = Q.IsInRange(CurrentTarget) && !castRE;

            #region Items

            if (CanCast("Items"))
            {
                if (d < Items.BOTRK.Range && Items.BOTRK.Cast(CurrentTarget))
                {
                    return;
                }

                if (d < Items.FQC.Range && Items.FQC.Cast(CurrentTarget))
                {
                    return;
                }
            }

            #endregion

            /*if (CastSecondW())
            {
                return;
            }*/

            if (eFirst && CastE())
            {
                return;
            }

            if (qFirst && CastQ())
            {
                return;
            }

            if (CastR())
            {
                return;
            }

            if (CastW())
            {
                return;
            }

            if (CastE()) {}
        }

        private static bool CastQ()
        {
            return CanCast("Q") && Q.IsReady() && Q.CanCast(CurrentTarget) && Q.Cast(CurrentTarget).IsCasted();
        }

        private static bool CastW()
        {
            var canCast = CanCast("W") && W.IsReady(1);
            var wRange = CurrentTarget.IsValidTarget(W.Range);
            var lowHealth = Player.HealthPercent <= Menu.Item("ComboWMinHP").GetValue<Slider>().Value;
            return canCast && wRange && !lowHealth && W.Cast(CurrentTarget).IsCasted();
        }

        private static bool CastSecondW()
        {
            var canCast = CanCast("W2") && W.IsReady(2);
            var isLowHP = Player.HealthPercent <= Menu.Item("MiscW2HP").GetValue<Slider>().Value;
            var moreEnemiesInRange = WBackPosition.Position.CountEnemiesInRange(600) > Player.CountEnemiesInRange(600);
            var isFleeing = Program.Orbwalker.ActiveMode == OrbwalkingMode.None;
            var spellDown = Menu.Item("ComboW2Spells").GetValue<bool>() && !Q.IsReady() && !E.IsReady() && !R.IsReady();
            var cast = canCast && (isLowHP || spellDown) && !moreEnemiesInRange && !isFleeing;
            return cast && W.Cast();
        }

        private static bool CastE()
        {
            if (!CanCast("E") || !E.IsReady() || !E.CanCast(CurrentTarget) || Player.IsDashing())
            {
                return false;
            }

            var pred = E.GetPrediction(CurrentTarget);
            return pred.Hitchance >= EHitChance && E.Cast(pred.CastPosition);
        }

        private static bool CastR()
        {
            var slot = GetMenuUlt();
            var canCast = CanCast("R") && R.IsReady(slot);

            if (!canCast)
            {
                return false;
            }

            if (slot == SpellSlot.Q && Q.IsInRange(CurrentTarget))
            {
                return R.Cast(SpellSlot.Q, CurrentTarget).IsCasted();
            }

            if (slot == SpellSlot.W && W.IsInRange(CurrentTarget))
            {
                return R.Cast(SpellSlot.W, CurrentTarget).IsCasted();
            }

            if (slot == SpellSlot.E && E.IsInRange(CurrentTarget))
            {
                E.Slot = SpellSlot.R;
                var cast = E.CastIfHitchanceEquals(CurrentTarget, EHitChance);
                E.Slot = SpellSlot.E;
                return cast;
            }

            return false;
        }

        public static bool CanCast(string spell)
        {
            return Menu.Item(Name + spell).GetValue<bool>();
        }

        private static float GetComboRange()
        {
            return Menu.Item("ComboEStart").GetValue<bool>() ? E.Range : Q.Range;
        }

        #region Events 

        private static void Game_OnGameUpdate(EventArgs args)
        {
            CurrentTarget = Utils.GetTarget(GetComboRange());

            if (!Enabled)
            {
                return;
            }

            //AOECombo();

            if (CurrentTarget.IsValidTarget(GetComboRange()))
            {
                ComboLogic();
            }
        }

        private static void AOECombo()
        {
            if (!Menu.Item("wComboEnabled").GetValue<bool>())
            {
                return;
            }

            var canFlash = Menu.Item("wComboFlash").GetValue<bool>() && Flash != null && Flash.IsReady();
            var spellsUp = W.IsReady() && R.IsReady();
            var range = canFlash ? W.Range + 550 : W.Range;
            var target = Utils.GetTarget(range);
            var minEnemies = Menu.Item("wComboEnemies").GetValue<Slider>().Value;

            if (!target.IsValidTarget(range))
            {
                return;
            }

            var enemies = target.ServerPosition.GetEnemiesInRange(260);

            if (enemies.Count < minEnemies)
            {
                return;
            }

            if (canFlash && spellsUp &&
                Player.Spellbook.CastSpell(Flash.Slot, Player.ServerPosition.Extend(target.ServerPosition, 550)))
            {
                return;
            }

            if (W.IsReady() && W.Cast(target).IsCasted())
            {
                return;
            }

            if (R.IsReady(SpellSlot.W) && R.Cast(SpellSlot.W, target).IsCasted()) {}
        }

        private static SpellSlot GetMenuUlt()
        {
            return (SpellSlot) Menu.Item("ComboRMode").GetValue<StringList>().SelectedIndex;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WBackPosition = new WPosition(sender);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(LeBlancWObject))
            {
                return;
            }

            WBackPosition = new WPosition();
        }

        #endregion
    }

    public class WPosition
    {
        public float EndTick;
        public Vector3 Position;
        public float StartTick;
        public GameObject Unit;

        public WPosition()
        {
            Position = Vector3.Zero;
            StartTick = 0;
            EndTick = 0;
        }

        public WPosition(GameObject unit)
        {
            Unit = unit;
            Position = unit.Position;
            StartTick = Environment.TickCount;
            EndTick = StartTick + 8000f;
        }

        public bool IsActive()
        {
            return Unit != null && Unit.IsValid && Environment.TickCount - EndTick < 0;
        }

        public float GetTime()
        {
            return (EndTick - Environment.TickCount) / 1000f;
        }
    }
}