using ITD.Content.Tiles.Furniture.BlueshroomGroves;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.BlueshroomGroves
{
    public class CharbleWorkBench : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture( ModContent.TileType<CharbleWorkBenchTile>(), 32, 16);
        }
    }
}
