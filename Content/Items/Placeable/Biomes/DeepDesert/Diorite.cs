using ITD.Content.Tiles.DeepDesert;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable.Biomes.DeepDesert
{
    public class Diorite : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<DioriteTile>());
        }
    }
}