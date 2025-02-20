using ITD.Content.Dusts;
using ITD.Content.Items.Placeable.Furniture.DeepDesert;

namespace ITD.Content.Tiles.Furniture.DeepDesert
{
    public class PyracottaChestTile : ITDChest
    {
        public override void SetStaticDefaultsSafe()
        {
            ITDSets.ITDChestMergeTo[Type] = ModContent.TileType<PyracottaDoubleChestTile>();
            DustType = ModContent.DustType<PyracottaDust>();
        }
        public override int ItemType => ModContent.ItemType<PyracottaChest>();
    }
}
