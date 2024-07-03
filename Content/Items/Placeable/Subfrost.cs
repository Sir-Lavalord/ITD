using Terraria;
using Terraria.ModLoader;
using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Placeable
{
    public class Subfrost : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SubfrostTile>());
            Item.width = 12;
            Item.height = 12;
        }
    }
}