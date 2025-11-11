using ITD.Content.Tiles.Furniture.DeepDesert;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert;

public class PyracottaBookcase : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToFurniture(ModContent.TileType<PyracottaBookcaseTile>(), 48, 64);
    }
}
