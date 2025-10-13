using ITD.Content.Tiles.DeepDesert;

namespace ITD.Content.Items.DevTools;

public class ReinforcedPegmatiteBricksItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<ReinforcedPegmatiteBricks>());
    }
}
