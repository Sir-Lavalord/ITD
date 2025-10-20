using ITD.Systems;

namespace ITD.DetoursIL;

public class World : DetourGroup
{
    public override void Load()
    {
        if (Main.dedServ)
            return;
        On_WorldGen.meteor += MeteorFall;
    }
    private static bool MeteorFall(On_WorldGen.orig_meteor orig, int i, int j, bool ignorePlayers) // Save if meteor has landed for natural CosJel spawn
    {
        bool result = orig(i, j, ignorePlayers);
        if (result)
        {
            ITDSystem.HasMeteorFallen = true;
        }
        return result;
    }
}
