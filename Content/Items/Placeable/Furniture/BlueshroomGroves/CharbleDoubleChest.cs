using ITD.Content.Tiles.Furniture.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Furniture.BlueshroomGroves;

public class CharbleDoubleChest : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<CharbleDoubleChestTile>(), 64, 32);
    }
}
