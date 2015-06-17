using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace AntiClone
{
    internal class Program
    {
        public static Dictionary<int, Render.Circle> MonitoredUnits = new Dictionary<int, Render.Circle>();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            GameObject.OnCreate += Obj_AI_Base_OnCreate;
            GameObject.OnDelete += Obj_AI_Base_OnDelete;
        }

        private static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Base;

            if (unit == null || !unit.IsValid)
            {
                return;
            }

            var enemy =
                ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(h => h.IsValid && h.IsEnemy && h.Name.Equals(unit.Name));

            if (enemy == null || !enemy.IsValid)
            {
                return;
            }

            Game.SendPing((PingCategory) 1, enemy);

            var circle = new Render.Circle(enemy, 200, Color.Red)
            {
                VisibleCondition = o => enemy.IsVisible && unit.IsValid
            };

            circle.Add();

            MonitoredUnits.Add(unit.NetworkId, circle);
        }

        private static void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            if (!MonitoredUnits.ContainsKey(sender.NetworkId))
            {
                return;
            }

            MonitoredUnits[sender.NetworkId].Dispose();
            MonitoredUnits.Remove(sender.NetworkId);
        }
    }
}