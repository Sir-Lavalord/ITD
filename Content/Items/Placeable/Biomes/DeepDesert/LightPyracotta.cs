using ITD.Content.Tiles.DeepDesert;

namespace ITD.Content.Items.Placeable.Biomes.DeepDesert;

public class LightPyracotta : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<LightPyracottaTile>());
    }
}
