using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Biomes.BlueshroomGroves;

public class BlueshroomStem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BlueshroomStemTile>());
        Item.width = 12;
        Item.height = 12;
    }
}
