using ITD.Content.Tiles.Furniture.Catacombs;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Items.Placeable.Furniture.Catacombs
{
    public class BlueCatacombPlatform : ModItem
    {
        public override string Texture => Placeholder.PHAxe;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BlueCatacombPlatformTile>());
            Item.width = 12;
            Item.height = 30;
        }
    }
    public class GreenCatacombPlatform : ModItem
    {
        public override string Texture => Placeholder.PHAxe;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GreenCatacombPlatformTile>());
            Item.width = 12;
            Item.height = 30;
        }
    }
    public class PinkCatacombPlatform : ModItem
    {
        public override string Texture => Placeholder.PHAxe;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PinkCatacombPlatformTile>());
            Item.width = 12;
            Item.height = 30;
        }
    }
}
