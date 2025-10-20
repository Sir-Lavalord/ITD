using ITD.Content.Walls.DeepDesert;

namespace ITD.Content.Items.DevTools;

public class ReinforcedPegmatiteBrickWallItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableWall(ModContent.WallType<ReinforcedPegmatiteBrickWallUnsafe>());
    }
}
