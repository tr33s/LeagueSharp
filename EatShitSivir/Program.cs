using LeagueSharp;
using LeagueSharp.Common;

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
                    var s = new Sivir();
                }
            };
        }
    }
}