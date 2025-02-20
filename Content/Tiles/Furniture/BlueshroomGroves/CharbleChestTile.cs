using ITD.Content.Items.Placeable.Furniture.BlueshroomGroves;

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
