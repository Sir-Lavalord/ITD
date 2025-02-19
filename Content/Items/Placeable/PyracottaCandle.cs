using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable
{
    public class PyracottaCandle : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(1, 1, ModContent.TileType<PyracottaCandleTile>());
        }
    }
}
