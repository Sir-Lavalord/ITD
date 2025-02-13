using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Tiles.LayersRework;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class CharbleBlock : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CharbleBlockTile>());
        }
    }
}
