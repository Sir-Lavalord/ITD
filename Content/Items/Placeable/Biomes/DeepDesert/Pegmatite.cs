using ITD.Content.Tiles.DeepDesert;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable.Biomes.DeepDesert
{
    public class Pegmatite : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PegmatiteTile>());
            Item.width = 12;
            Item.height = 12;
        }
    }
}
