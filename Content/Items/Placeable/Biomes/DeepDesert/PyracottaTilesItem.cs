using ITD.Content.Tiles.DeepDesert;

namespace ITD.Content.Items.Placeable.Biomes.DeepDesert
{
    public class LightPyracottaTilesItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LightPyracottaTiles>());
        }
    }
    public class DarkPyracottaTilesItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<DarkPyracottaTiles>());
        }
    }
}
