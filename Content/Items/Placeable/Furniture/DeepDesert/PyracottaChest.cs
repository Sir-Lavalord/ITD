using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert
{
    public class PyracottaChest : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(2, 2, ModContent.TileType<PyracottaChestTile>());
        }
    }
}
