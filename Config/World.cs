using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ITD.Config
{
    public partial class ITDServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        public bool ResizeDesertForDeepDesert;

        // doesn't do anything for now
        [DefaultValue(false)]
        public bool AutoMergeChestsIntoDoubleChest;
    }
}
