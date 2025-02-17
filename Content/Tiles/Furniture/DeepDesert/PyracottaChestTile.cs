using ITD.Content.Items.Placeable;
using ITD.Systems.DataStructures;

namespace ITD.Content.Tiles.Furniture.DeepDesert
{
    public class PyracottaChestTile : ITDChest
    {
        public override Point8 Dimensions => new(3, 2);
        public override int ItemType => ModContent.ItemType<PyracottaChest>();
    }
}
