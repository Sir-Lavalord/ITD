using ITD.Content.Dusts;

namespace ITD.Content.Tiles.Furniture.DeepDesert;

public class PyracottaDoor : ITDDoor
{
    public override int[] DropItems => [ModContent.ItemType<Items.Placeable.Furniture.DeepDesert.PyracottaDoor>()];
    public override int[] DustTypes => [ModContent.DustType<PyracottaDust>()];
    public override Color?[] MapColors => [new(171, 77, 57)];
}
