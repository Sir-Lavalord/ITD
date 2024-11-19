using ITD.Content.Tiles.DeepDesert;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class LightPyracotta : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LightPyracottaTile>());
        }
    }
}
