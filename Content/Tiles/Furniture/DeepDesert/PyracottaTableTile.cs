using ITD.Utilities;

namespace ITD.Content.Tiles.Furniture.DeepDesert
{
    public class PyracottaTableTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            this.DefaultToTable(new(171, 77, 57), true, false);
        }
    }
}
