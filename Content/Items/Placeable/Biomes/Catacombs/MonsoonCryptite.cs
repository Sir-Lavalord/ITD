using ITD.Content.Tiles.Catacombs;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable.Biomes.Catacombs
{
    public class MonsoonCryptite : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<MonsoonCryptiteTile>());
        }
    }
}
