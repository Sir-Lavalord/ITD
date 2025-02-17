using ITD.Content.Tiles.Furniture.BlueshroomGroves;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable
{
    public class CharbleDoubleChest : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(4, 2, ModContent.TileType<CharbleDoubleChestTile>());
        }
    }
}
