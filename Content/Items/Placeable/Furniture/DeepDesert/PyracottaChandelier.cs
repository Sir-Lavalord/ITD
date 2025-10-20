using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class PyracottaChandelier : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<PyracottaChandelierTile>(), 30, 38);
    }
}
