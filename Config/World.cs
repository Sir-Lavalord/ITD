using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ITD.Config;

public partial class ITDServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(true)]
    public bool ResizeDesertForDeepDesert;

    [DefaultValue(false)]
    public bool AutoMergeChestsIntoDoubleChest;
}
