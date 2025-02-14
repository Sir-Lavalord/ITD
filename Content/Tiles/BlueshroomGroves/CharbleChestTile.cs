using ITD.Content.Items.Placeable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class CharbleChestTile : ITDChest
    {
        public override int ItemType => ModContent.ItemType<CharbleChest>();

        public override int KeyType => ItemID.DirtBlock;
    }
}
