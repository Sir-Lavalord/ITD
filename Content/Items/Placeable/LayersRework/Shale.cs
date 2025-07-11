using ITD.Content.Tiles.LayersRework;

namespace ITD.Content.Items.Placeable.LayersRework
{
    public class Shale : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ShaleTile>());
        }
    }
}
