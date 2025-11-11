using Daybreak.Common.Features.Hooks;
using ITD.Systems;

namespace ITD.DetoursIL;

public static class WorldHooks
{
    [OnLoad]
    public static void Load()
    {
        On_WorldGen.meteor += static (orig, i, j, ignorePlayers) =>
        {
            bool result = orig(i, j, ignorePlayers);
            if (result)
            {
                ITDSystem.HasMeteorFallen = true;
            }
            return result;
        };
    }
}
