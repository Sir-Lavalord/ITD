using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;

namespace ITD
{
    public class DownedBossSystem : ModSystem
    {
        // This system is modeled on Calamity's DownedBossSystem. I think this'll be good on the long term since the amount of bosses in this mod is ginormous.
        // Bosses
        internal static bool _downedCosJel = false;
        public static bool downedCosJel
        {
            get => _downedCosJel;
            set
            {
                if (!value)
                    _downedCosJel = false;
                else
                    NPC.SetEventFlagCleared(ref _downedCosJel, -1);
            }
        }
        internal static void ResetFlags()
        {
            downedCosJel = false;
        }
        public override void OnWorldLoad() => ResetFlags();
        public override void OnWorldUnload() => ResetFlags();
        public override void SaveWorldData(TagCompound tag)
        {
            List<string> downed = [];
            if (downedCosJel)
                downed.Add("cosJel");
            tag["downedFlags"] = downed;
        }
        public override void LoadWorldData(TagCompound tag)
        {
            IList<string> downed = tag.GetList<string>("downedFlags");
            downedCosJel = downed.Contains("cosJel");
        }
    }
}
