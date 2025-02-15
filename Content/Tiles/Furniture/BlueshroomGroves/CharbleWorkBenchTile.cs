using ITD.Utilities;

namespace ITD.Content.Tiles.Furniture.BlueshroomGroves
{
    public class CharbleWorkBenchTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            this.DefaultToWorkbench();
            DustType = DustID.Marble;
        }
    }
}
