using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using TreeLib.Extensions;

namespace jesuisFiora
{
    internal static class ItemManager
    {
        public static Items.Item Botrk
        {
            get { return ItemData.Blade_of_the_Ruined_King.GetItem(); }
        }

        public static Items.Item Cutlass
        {
            get { return ItemData.Bilgewater_Cutlass.GetItem(); }
        }

        public static Items.Item Youmuus
        {
            get { return ItemData.Youmuus_Ghostblade.GetItem(); }
        }

        public static Items.Item Tiamat
        {
            get { return ItemData.Tiamat_Melee_Only.GetItem(); }
        }

        public static Items.Item RavenousHydra
        {
            get { return ItemData.Ravenous_Hydra_Melee_Only.GetItem(); }
        }

        public static Items.Item TitanicHydra
        {
            get { return new Items.Item(3748, 385); }
        }

        public static bool IsValidAndReady(this Items.Item item)
        {
            return item != null && item.IsReady();
        }

        public static bool IsActive()
        {
            var name = "Items" + Program.Orbwalker.ActiveMode.GetModeString();
            var item = Program.Menu.Item(name);
            return item != null && item.IsActive();
        }
    }
}