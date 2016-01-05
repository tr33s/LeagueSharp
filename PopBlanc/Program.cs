using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;

namespace PopBlanc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (ObjectManager.Player.ChampionName.Equals("Leblanc"))
                {
                    Bootstrap.Initialize();
                    var s = new LeBlanc();
                }
            };
        }
    }
}