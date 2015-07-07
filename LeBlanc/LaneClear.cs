using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class LaneClear
    {
        private const string Name = "LaneClear";
        public static Menu LocalMenu;

        static LaneClear()
        {
            #region Menu

            var laneclear = new Menu("Farm Settings", "LaneClear");

            var lcQ = laneclear.AddMenu("Q", "Q");
            lcQ.AddBool("LaneClearQ", "Use Q");
            lcQ.AddSlider("LaneClearQMana", "Minimum Q Mana Percent", 30);


            var lcW = laneclear.AddMenu("W", "W");
            lcW.AddBool("LaneClearW", "Use W");
            lcW.AddBool("LaneClearRW", "Use RW");
            lcW.AddSlider("LaneClearWHits", "Min Enemies Hit", 2, 0, 5);
            lcW.AddSlider("LaneClearWMana", "Minimum W Mana Percent", 30);

            laneclear.AddKeyBind("LaneClearKey", "Farm Key", (byte) 'V');

            #endregion

            LocalMenu = laneclear;

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

        private static Spell R
        {
            get { return Spells.E; }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

            if (CastQ())
            {
                return;
            }

            if (CastW())
            {
                return;
            }

            if (CastW(true)) {}
        }

        public static bool CastQ()
        {
            var canCast = CanCast("Q") && Q.IsReady();
            var isLowMana = Player.ManaPercent < LocalMenu.Item("LaneClearQMana").GetValue<Slider>().Value;

            if (!canCast || isLowMana)
            {
                return false;
            }

            var unit =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        minion =>
                            minion.IsValidTarget(Q.Range) &&
                            minion.Health < Player.GetDamageSpell(minion, SpellSlot.Q).CalculatedDamage);

            return unit.IsValidTarget(Q.Range) && Q.Cast(unit).IsCasted();
        }

        public static bool CastW(bool ult = false)
        {
            var canCast = CanCast("W") && W.IsReady(1);
            var canCastUlt = ult && CanCast("RW") && R.IsReady(SpellSlot.W);
            var isLowMana = Player.ManaPercent <= Menu.Item("LaneClearWMana").GetValue<Slider>().Value;
            var minions = MinionManager.GetMinions(W.Range).Select(m => m.ServerPosition.To2D()).ToList();
            var minionPrediction = MinionManager.GetBestCircularFarmLocation(minions, W.Width, W.Range);
            var castPosition = minionPrediction.Position.To3D();
            var notEnoughHits = minionPrediction.MinionsHit < Menu.Item("LaneClearWHits").GetValue<Slider>().Value;

            if (notEnoughHits)
            {
                return false;
            }

            if (canCastUlt)
            {
                return R.Cast(SpellSlot.W, castPosition);
            }

            return canCast && !isLowMana && W.Cast(castPosition);
        }

        public static bool CanCast(string spell)
        {
            return LocalMenu.Item(Name + spell).GetValue<bool>();
        }
    }
}