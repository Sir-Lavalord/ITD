using ITD.Content.Items.Placeable.Furniture.Catacombs;

namespace ITD.Content.Tiles.Furniture.Catacombs
{
    public class CatacombDoor : ITDDoor
    {
        public override int[] DropItems => [ModContent.ItemType<BlueCatacombDoor>(), ModContent.ItemType<PinkCatacombDoor>(), ModContent.ItemType<GreenCatacombDoor>()];
        public override int[] DustTypes => [DustID.Shadowflame];
    }
}
