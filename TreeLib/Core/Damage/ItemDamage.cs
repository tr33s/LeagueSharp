using System.Linq;
using LeagueSharp;
using TreeLib.Managers;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace TreeLib.Core.Damage
{
    public static class ItemDamage
    {
        public static float GetItemDamage(this Obj_AI_Base target, bool predicted = false)
        {
            var d = 0f;

            foreach (var item in ItemManager.ItemList.Where(i => i.IsValidAndReady()))
            {
                try
                {
                    //d += ObjectManager.Player.GetItemDamage(target, );
                }
                catch {}
            }

            if (ItemData.Ludens_Echo.GetItem() != null)
            {
                var b = ObjectManager.Player.GetBuff("itemmagicshankcharge");
                if (b != null && b.IsActive && b.Count >= (predicted ? 70 : 95))
                {
                    d += 100 + ObjectManager.Player.TotalMagicalDamage * .1f;
                }
            }

            return d;
        }
    }
}