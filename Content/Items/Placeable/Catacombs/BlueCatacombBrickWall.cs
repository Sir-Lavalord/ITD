using ITD.Content.Walls.Catacombs;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable.Catacombs
{
    public class BlueCatacombBrickWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BlueCatacombBrickWallTile>());
        }
    }
}
