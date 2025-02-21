using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert
{
    public class PyracottaChair : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(ModContent.TileType<PyracottaChairTile>(), 16, 32);
        }
    }
}
