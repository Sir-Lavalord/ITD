using ITD.Content.Dusts;
using ITD.Content.Items.Placeable.Furniture.DeepDesert;

namespace ITD.Content.Tiles.Furniture.DeepDesert;

public class PyracottaChairTile : ITDChair
{
    public override void SetStaticChairDefaults()
    {
        DustType = ModContent.DustType<PyracottaDust>();
        MapColor = new(171, 77, 57);
        ChairItem = [ModContent.ItemType<PyracottaChair>()];
    }
}
