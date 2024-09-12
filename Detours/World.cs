using Terraria.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using ITD.Systems;

namespace ITD.Detours
{
    public class World : ModSystem
    {
        public override void Load()
        {
            if (Main.dedServ)
                return;
            On_WorldGen.meteor += MeteorFall;
        }
        public override void Unload()
        {
            if (Main.dedServ)
                return;
            On_WorldGen.meteor -= MeteorFall;
        }
        private static bool MeteorFall(On_WorldGen.orig_meteor orig, int i, int j, bool ignorePlayers) // Save if meteor has landed for natural CosJel spawn
        {
            bool result = orig(i, j, ignorePlayers);
            if (result)
            {
                ITDSystem.hasMeteorFallen = true;
            }
            return result;
        }
    }
}
