using ITD.Content.Walls.Catacombs;

namespace ITD.Content.Items.Placeable.Biomes.Catacombs;

public class GreenCatacombBrickWall : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<GreenCatacombBrickWallSafe>());
    }
}
