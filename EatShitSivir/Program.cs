using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;

namespace EatShitSivir
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (ObjectManager.Player.ChampionName.Equals("Sivir"))
                {
                    Bootstrap.Initialize();
                    var s = new Sivir();
                }
            };
        }
    }
}