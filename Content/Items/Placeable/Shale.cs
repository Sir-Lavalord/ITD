using ITD.Content.Tiles;
using Terraria.ModLoader;

namespace ITD.Content.Items
{
    public class Shale : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ShaleTile>());
            Item.width = 12;
            Item.height = 12;
        }
    }
}
