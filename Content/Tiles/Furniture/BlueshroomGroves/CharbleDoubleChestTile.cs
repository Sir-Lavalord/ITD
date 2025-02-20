using ITD.Content.Items.Placeable.Furniture.BlueshroomGroves;
using ITD.Systems.DataStructures;

namespace ITD.Content.Tiles.Furniture.BlueshroomGroves
{
    public class CharbleDoubleChestTile : ITDChest
    {
        public override Point8 Dimensions => new(4, 2);
        public override Point8 StorageDimensions => new(10, 8);
        public override int ItemType => ModContent.ItemType<CharbleChest>();
        public override void SetStaticDefaultsSafe()
        {
            DustType = DustID.Marble;
        }
    }
}
