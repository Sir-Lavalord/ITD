using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace ITD.Config
{
    public partial class ITDClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(100)]
        [DrawTicks]
        [Slider]
        [Range(0, 100)]
        public int ScreenshakeIntensity;
    }
}
