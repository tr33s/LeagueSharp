using System.Collections.Generic;
using LeagueSharp;

namespace jesuisFiora
{
    internal static class GameObjects
    {
        public static IEnumerable<Obj_GeneralParticleEmitter> GetParticleEmitters()
        {
            return LeagueSharp.SDK.GameObjects.ParticleEmitters;
        }
    }
}