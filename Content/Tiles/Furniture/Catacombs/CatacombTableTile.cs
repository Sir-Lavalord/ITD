using ITD.Utilities;

namespace ITD.Content.Tiles.Furniture.Catacombs
{
    public class CatacombTableTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            this.DefaultToTable(new Color(30, 63, 96), false, true);
            DustType = DustID.Shadowflame;
        }
    }
}
