using ITD.Content.Tiles.Furniture.Catacombs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class BlueCatacombLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombLampTile>());
            Item.width = 12;
            Item.height = 30;
        }
    }
    public class GreenCatacombLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombLampTile>(), 1);
            Item.width = 12;
            Item.height = 30;
        }
    }
    public class PinkCatacombLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombLampTile>(), 2);
            Item.width = 12;
            Item.height = 30;
        }
    }
}
