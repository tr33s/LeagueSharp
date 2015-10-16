using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace KindredSpirits
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

        public static bool IsValidAndReady(this Items.Item item)
        {
            return item != null && item.IsReady();
        }
    }
}