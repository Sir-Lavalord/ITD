using ITD.Content.Items.Placeable;

namespace ITD.Content.Tiles.Furniture.BlueshroomGroves
{
    public class CharbleChestTile : ITDChest
    {
        public override int ItemType => ModContent.ItemType<CharbleChest>();
        public override void SetStaticDefaultsSafe()
        {
            DustType = DustID.Marble;
            ITDSets.ITDChestMergeTo[Type] = ModContent.TileType<CharbleDoubleChestTile>();
        }
    }
}
