using ITD.Content.Tiles.Furniture.DeepDesert;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert
{
    public class PyracottaLantern : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(ModContent.TileType<PyracottaLanternTile>(), 18, 40);
        }
    }
}
