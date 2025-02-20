using ITD.Content.Walls.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Biomes.BlueshroomGroves
{
    public class SmoothedCharbleWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<SmoothedCharbleWallPlaced>());
        }
    }
}
