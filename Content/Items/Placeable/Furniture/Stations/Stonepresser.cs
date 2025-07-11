using ITD.Content.Tiles.Furniture.Stations;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.Stations
{
    public class Stonepresser : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(ModContent.TileType<StonepresserTile>(), 48, 64);
        }
    }
}
