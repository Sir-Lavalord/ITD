using ITD.Content.Tiles.Furniture.DeepDesert;

namespace ITD.Content.Items.Placeable
{
    public class PyracottaChest : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<PyracottaChestTile>());
        }
    }
}
