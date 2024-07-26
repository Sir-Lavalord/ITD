using ITD.Content.Tiles.BlueshroomGroves;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class BlueshroomCapBlock : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BlueshroomCapBlockTile>());
        }
    }
}
