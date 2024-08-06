using ITD.Content.Walls.Catacombs;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class GreenCatacombBrickWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GreenCatacombBrickWallTile>());
        }
    }
}
