using ITD.Content.Walls;
using ITD.Content.Walls.BlueshroomGroves;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
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
