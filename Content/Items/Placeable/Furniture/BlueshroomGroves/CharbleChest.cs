using ITD.Content.Tiles.Furniture.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Furniture.BlueshroomGroves;

public class CharbleChest : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<CharbleChestTile>(), 32, 32);
    }
}
