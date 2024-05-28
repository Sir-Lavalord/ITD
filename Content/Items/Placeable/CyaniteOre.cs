using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Tiles;

namespace ITD.Content.Items
{
    public class CyaniteOre : ModItem
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