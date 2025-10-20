using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class PyracottaCandle : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<PyracottaCandleTile>(), 16, 16);
    }
}
