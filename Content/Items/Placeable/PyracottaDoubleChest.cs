using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable
{
    public class PyracottaDoubleChest : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(4, 2, ModContent.TileType<PyracottaDoubleChestTile>());
        }
    }
}
