using ITD.Content.Tiles.Furniture.BlueshroomGroves;
using ITD.Utilities;

namespace ITD.Content.Items.Placeable.Furniture.BlueshroomGroves
{
    public class CharbleWorkBench : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToFurniture(2, 1, ModContent.TileType<CharbleWorkBenchTile>());
        }
    }
}
