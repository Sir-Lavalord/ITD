using ITD.Content.Tiles.Furniture.DeepDesert;

namespace ITD.Content.Items.Placeable.Furniture.DeepDesert
{
    public class PyracottaLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PyracottaLampTile>());
            Item.width = 12;
            Item.height = 30;
        }
    }
}
