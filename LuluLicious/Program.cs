using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Core;

namespace LuluLicious
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += eventArgs =>
            {
                if (ObjectManager.Player.ChampionName == "Lulu")
                {
                    Bootstrap.Initialize();
                    var s = new Lulu();
                }
            };
        }
    }
}