using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace TreeLib.Managers
{
    public static class ItemManager
    {
        public static List<Items.Item> ItemList;

        static ItemManager()
        {
            ItemList = new List<Items.Item> { Botrk, Cutlass, Youmuus, Tiamat, RavenousHydra, TitanicHydra, LudensEcho };
        }

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

        public static Items.Item LudensEcho
        {
            get { return ItemData.Ludens_Echo.GetItem(); }
        }

        public static int GuinsooStack
        {
            get { return ObjectManager.Player.GetBuffCount("GuinsooRageblade"); }
        }

        public static bool IsValidAndReady(this Items.Item item)
        {
            return item != null && item.IsReady();
        }
    }
}