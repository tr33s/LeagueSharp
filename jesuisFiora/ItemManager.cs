using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using TreeLib.Extensions;

namespace jesuisFiora
{
    internal static class ItemManager
    {
        public static Items.Item Botrk => ItemData.Blade_of_the_Ruined_King.GetItem();
        public static Items.Item Cutlass => ItemData.Bilgewater_Cutlass.GetItem();
        public static Items.Item Youmuus => ItemData.Youmuus_Ghostblade.GetItem();
        public static Items.Item Tiamat => ItemData.Tiamat_Melee_Only.GetItem();
        public static Items.Item RavenousHydra => ItemData.Ravenous_Hydra_Melee_Only.GetItem();
        public static Items.Item TitanicHydra => new Items.Item(3748, 385);

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