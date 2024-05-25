using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Tiles;

namespace ITD.Content.Items
{
    public class CosmicJellyfishRelic : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<CosmicJellyfishRelicTile>(), 0);

            Item.width = 30;
            Item.height = 40;
            Item.rare = ItemRarityID.Master;
            Item.master = true;
            Item.value = Item.buyPrice(0, 5);
        }
    }
}