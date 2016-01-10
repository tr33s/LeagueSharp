using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;

namespace Staberina
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (ObjectManager.Player.ChampionName == "Katarina")
                {
                    Bootstrap.Initialize();
                    var s = new Katarina();
                }
            };
        }
    }
}