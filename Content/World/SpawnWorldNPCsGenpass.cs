using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace ITD.Content.World
{
    public class SpawnWorldNPCsGenpass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            SpawnMudkarp();
        }
        private static void SpawnMudkarp()
        {
            
        }
    }
}
