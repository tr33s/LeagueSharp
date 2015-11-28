using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TentacleBabeIllaoi
{
    internal class TentacleManager
    {
        public static List<Obj_AI_Minion> TentacleList = new List<Obj_AI_Minion>();
        public static int TentacleAutoAttackRange = 190;

        public static void Initialize()
        {
            foreach (var tentacle in
                ObjectManager.Get<Obj_AI_Minion>()
                    .Where(
                        t =>
                            t.IsValid && t.IsVisible && t.IsAlly &&
                            t.CharData.BaseSkinName.ToLower().Equals("illaoiminion")))
            {
                TentacleList.Add(tentacle);
            }

            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var tentacle in TentacleList.Where(t => t.IsValid && t.IsVisible && t.IsHPBarRendered))
            {
                Render.Circle.DrawCircle(tentacle.ServerPosition, TentacleAutoAttackRange, Color.DarkBlue);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var minion = sender as Obj_AI_Minion;
            if (minion == null || !minion.IsValid || !minion.IsAlly ||
                !minion.CharData.BaseSkinName.ToLower().Equals("illaoiminion"))
            {
                return;
            }

            TentacleList.Add(minion);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            TentacleList.RemoveAll(t => t.NetworkId.Equals(sender.NetworkId));
        }
    }
}