using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Clone
    {
        public static Menu LocalMenu;
        public static Obj_AI_Base Pet;

        static Clone()
        {
            #region Menu

            var clone = new Menu("Clone Settings", "Clone");
            clone.AddBool("CloneEnabled", "Enabled");
            clone.AddList("CloneMode", "Mode", new[] { "To Player", "To Target", "Away from Player" });

            #endregion

            LocalMenu = clone;

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Menu.Item("CloneEnabled").GetValue<bool>(); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(Player.Name))
            {
                return;
            }

            Pet = sender as Obj_AI_Base;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals(Player.Name))
            {
                return;
            }

            Pet = null;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            try
            {
                if (!Enabled)
                {
                    return;
                }

                var pet = Pet; //Player.Pet as Obj_AI_Base;
                var mode = Menu.Item("CloneMode").GetValue<StringList>().SelectedIndex;


                if (pet == null || !pet.IsValid || pet.IsDead || pet.Health < 1)
                {
                    return;
                }

                var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Physical, true, null, pet.Position);

                if (mode == 1 &&
                    !(pet.CanAttack && !pet.IsWindingUp && !pet.Spellbook.IsAutoAttacking &&
                      target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(pet))))
                {
                    mode = 0;
                }

                switch (mode)
                {
                    case 0: // toward player
                        var pos = Player.GetWaypoints().Count > 1
                            ? Player.GetWaypoints()[1].To3D()
                            : Player.ServerPosition;
                        Utility.DelayAction.Add(200, () => { pet.IssueOrder(GameObjectOrder.MovePet, pos); });
                        break;
                    case 1: //toward target
                        pet.IssueOrder(GameObjectOrder.AutoAttackPet, target);
                        break;
                    case 2: //away from player
                        Utility.DelayAction.Add(
                            100,
                            () =>
                            {
                                pet.IssueOrder(
                                    GameObjectOrder.MovePet,
                                    (pet.Position + 500 * ((pet.Position - Player.Position).Normalized())));
                            });
                        break;
                }
            }
            catch {}
        }
    }
}