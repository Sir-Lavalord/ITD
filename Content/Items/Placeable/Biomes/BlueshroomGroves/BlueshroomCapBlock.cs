using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Biomes.BlueshroomGroves;

public class BlueshroomCapBlock : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BlueshroomCapBlockTile>());
    }
}
