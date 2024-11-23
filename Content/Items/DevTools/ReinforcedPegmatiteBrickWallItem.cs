using ITD.Content.Walls.DeepDesert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ITD.Content.Items.DevTools
{
    public class ReinforcedPegmatiteBrickWallItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<ReinforcedPegmatiteBrickWallUnsafe>());
        }
    }
}
