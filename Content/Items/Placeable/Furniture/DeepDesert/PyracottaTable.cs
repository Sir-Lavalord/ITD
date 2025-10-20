using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class PyracottaTable : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<PyracottaTableTile>(), 48, 32);
    }
}
