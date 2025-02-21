using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert
{
    public class PyracottaDresser : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(ModContent.TileType<PyracottaDresserTile>(), 48, 32);
        }
    }
}
