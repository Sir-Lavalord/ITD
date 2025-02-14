using ITD.Content.Tiles.BlueshroomGroves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Content.Items.Placeable
{
    public class CharblePlatform : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CharblePlatformTile>());
        }
    }
}
