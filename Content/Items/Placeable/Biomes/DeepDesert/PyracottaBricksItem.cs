using ITD.Content.Tiles.DeepDesert;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable.Biomes.DeepDesert
{
    public class LightPyracottaBricksItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LightPyracottaBricks>());
        }
    }
    public class DarkPyracottaBricksItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<DarkPyracottaBricks>());
        }
    }
}
