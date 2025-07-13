using ITD.Content.Tiles.Furniture.Catacombs;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Items.Placeable.Furniture.Catacombs
{
    public class BlueCatacombCarvedEmblem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombCarvedEmblemTile>());
            Item.width = 34;
            Item.height = 34;
        }
    }
    public class GreenCatacombCarvedEmblem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombCarvedEmblemTile>(), 2);
            Item.width = 34;
            Item.height = 34;
        }
    }
    public class PinkCatacombCarvedEmblem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CatacombCarvedEmblemTile>(), 1);
            Item.width = 34;
            Item.height = 34;
        }
    }
}
