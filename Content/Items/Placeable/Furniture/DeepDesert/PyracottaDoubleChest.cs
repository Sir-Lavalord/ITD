using ITD.Content.Tiles.Furniture.DeepDesert;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class PyracottaDoubleChest : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<PyracottaDoubleChestTile>(), 64, 32);
    }
}
