using Terraria.ID;
using Microsoft.Xna.Framework;

namespace ITD.Content.Tiles.Furniture.Catacombs
{
    public class CatacombTableTile : ITDTable
    {
        public override void SetStaticTableDefaults()
        {
            DustType = DustID.Shadowflame;
            MapColor = new Color(30, 63, 96);
        }
    }
}
