using ITD.Content.Items.Placeable;
using ITD.Systems.DataStructures;

namespace ITD.Content.Tiles.Furniture.DeepDesert
{
    public class PyracottaChestTile : ITDChest
    {
        public override void SetStaticDefaultsSafe()
        {
            ITDSets.ITDChestMergeTo[Type] = ModContent.TileType<PyracottaDoubleChestTile>();
        }
        public override int ItemType => ModContent.ItemType<PyracottaChest>();
    }
}
