using ITD.Content.Dusts;
using ITD.Content.Items.Placeable.Furniture.DeepDesert;
using ITD.Systems.DataStructures;

namespace ITD.Content.Tiles.Furniture.DeepDesert;

public class PyracottaDoubleChestTile : ITDChest
{
    public override int ItemType => ModContent.ItemType<PyracottaChest>();
    public override Point8 Dimensions => new(4, 2);
    public override Point8 StorageDimensions => new(11, 8);
    public override void SetStaticDefaultsSafe()
    {
        DustType = ModContent.DustType<PyracottaDust>();
    }
}
