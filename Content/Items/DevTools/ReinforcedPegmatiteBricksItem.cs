using ITD.Content.Tiles.DeepDesert;
using Terraria.ModLoader;

namespace ITD.Content.Items.DevTools
{
    public class ReinforcedPegmatiteBricksItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ReinforcedPegmatiteBricks>());
        }
    }
}
