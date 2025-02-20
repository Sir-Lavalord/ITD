using ITD.Content.Walls;

namespace ITD.Content.Items.Placeable.Biomes.Catacombs
{
    public class BlueCatacombBrickWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<BlueCatacombBrickWallSafe>());
        }
    }
}
