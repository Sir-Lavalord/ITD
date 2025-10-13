using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ITD.Config;

public partial class ITDClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(100)]
    [Slider]
    [Range(0, 100)]
    public int ScreenshakeIntensity;
}
