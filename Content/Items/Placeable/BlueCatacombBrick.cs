using ITD.Content.Tiles.LayersRework;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class BlueCatacombBrick : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BlueCatacombBrickTile>());
        }
    }
}
