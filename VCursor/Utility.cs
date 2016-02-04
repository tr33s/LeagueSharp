using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace VCursor
{
    internal static class Utility
    {
        public static Vector3 ToWorldPoint(this Vector2 position)
        {
            return Drawing.ScreenToWorld(position);
        }

        public static Vector2 ToScreenPoint(this Vector3 position)
        {
            return Drawing.WorldToScreen(position);
        }

        public static Vector2 ToScreenPoint(this Vector2 position)
        {
            return Drawing.WorldToScreen(position.To3D());
        }

        public static bool HoverShop(this Vector2 position)
        {
            var shop =
                ObjectHandler.Get<Obj_Shop>().FirstOrDefault(o => o.Position.Distance(position.ToWorldPoint()) < 300);
            return ObjectManager.Player.InShop() && shop != null && !MenuGUI.IsShopOpen;
        }

        public static bool HoverAllyTurret(this Vector2 position)
        {
            var allyTurret =
                ObjectManager.Get<Obj_AI_Turret>()
                    .FirstOrDefault(o => o.IsAlly && o.Distance(position.ToWorldPoint()) < 120 && o.Health < 9999);
            return allyTurret != null;
        }

        public static bool HoverEnemy(this Vector2 position)
        {
            var enemy =
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(o => o.IsValidTarget(150, true, position.ToWorldPoint()));
            return enemy.IsValidTarget();
        }
    }
}