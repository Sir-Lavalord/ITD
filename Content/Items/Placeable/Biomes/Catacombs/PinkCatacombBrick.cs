using ITD.Content.Tiles.Catacombs;

namespace ITD.Content.Items.Placeable.Biomes.Catacombs;

public class PinkCatacombBrick : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<PinkCatacombBrickTile>());
    }
}
