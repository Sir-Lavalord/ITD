﻿using ITD.Content.Tiles.Furniture.Catacombs;

namespace ITD.Content.Items.Placeable.Furniture.Catacombs
{
    public class BlueCatacombTable : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombTableTile>());
            Item.width = 38;
            Item.height = 24;
        }
    }
    public class GreenCatacombTable : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombTableTile>(), 1);
            Item.width = 38;
            Item.height = 24;
        }
    }
    public class PinkCatacombTable : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombTableTile>(), 2);
            Item.width = 38;
            Item.height = 24;
        }
    }
}
