using ITD.Content.Tiles;
using Terraria.ModLoader;

namespace ITD.Content.Items
{
    public class Depthrock : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<DepthrockTile>());
            Item.width = 12;
            Item.height = 12;
        }
    }
}
