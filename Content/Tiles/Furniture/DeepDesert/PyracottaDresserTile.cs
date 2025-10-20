using ITD.Content.Dusts;
using ITD.Content.Items.Placeable.Furniture.DeepDesert;

namespace ITD.Content.Tiles.Furniture.DeepDesert;

public class PyracottaDresserTile : ITDDresser
{
    public override int ItemType => ModContent.ItemType<PyracottaDresser>();
    public override void SetStaticDefaultsSafe()
    {
        DustType = ModContent.DustType<PyracottaDust>();
    }
}
