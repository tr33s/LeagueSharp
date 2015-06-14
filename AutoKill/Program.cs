using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace AutoKill
{
    internal class Program
    {
        public static Menu Menu;
        public static Spell Ignite;
        public static Spell Smite;

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("AutoKill", "AutoKill", true);
            Menu.AddBool("Ignite", "Cast Ignite");
            Menu.AddBool("Smite", "Cast Smite");
            Menu.AddBool("AA", "Use AutoAttack");

            Menu.AddToMainMenu();

            var igniteSlot = Player.GetSpellSlot("summonerdot");

            if (igniteSlot.IsValid())
            {
                Ignite = new Spell(igniteSlot, 600);
                Ignite.SetTargetted(.172f, 20);
            }

            var smite = Player.Spellbook.Spells.FindAll(h => h.Name.ToLower().Contains("smite")).FirstOrDefault();

            if (smite != null && smite.Slot.IsValid())
            {
                Smite = new Spell(smite.Slot, 760);
                Smite.SetTargetted(.333f, 20);
            }

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Menu.Item("AA").GetValue<bool>() && AutoAttack())
            {
                return;
            }

            if (Menu.Item("Smite").GetValue<bool>() && CastSmite())
            {
                return;
            }

            if (Menu.Item("Ignite").GetValue<bool>() && CastIgnite()) {}
        }

        private static bool AutoAttack()
        {
            if (!Player.CanAttack || Player.IsChannelingImportantSpell() || Player.Spellbook.IsAutoAttacking)
            {
                return false;
            }

            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            h.IsValidTarget(Player.GetAutoAttackRange()) &&
                            h.Health < Player.GetAutoAttackDamage(h, true))
                    .Any(enemy => Player.IssueOrder(GameObjectOrder.AutoAttack, enemy));
        }

        private static bool CastIgnite()
        {
            if (Ignite == null || !Ignite.IsReady())
            {
                return false;
            }

            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            h.IsValidTarget(Ignite.Range) &&
                            h.Health < Player.GetSummonerSpellDamage(h, Damage.SummonerSpell.Ignite))
                    .Any(enemy => Ignite.Cast(enemy).IsCasted());
        }

        private static bool CastSmite()
        {
            if (Smite == null || !Smite.IsCastableOnChampion() || !Smite.Instance.IsReady() || Smite.Instance.Ammo < 1)
            {
                return false;
            }

            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        h =>
                            h.IsValidTarget(Smite.Range) &&
                            h.Health < Player.GetSummonerSpellDamage(h, Damage.SummonerSpell.Smite))
                    .Any(h => Smite.Cast(h).IsCasted());
        }
    }
}