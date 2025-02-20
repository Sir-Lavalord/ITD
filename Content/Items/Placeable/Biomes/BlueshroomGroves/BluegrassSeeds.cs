using ITD.Utilities;
using ITD.Content.Tiles.BlueshroomGroves;

namespace ITD.Content.Items.Placeable.Biomes.BlueshroomGroves
{
    public class BluegrassSeeds : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.DefaultToSeeds();
        }
        public override bool? UseItem(Player player) => Helpers.UseItem_PlaceSeeds(player, ModContent.TileType<Bluegrass>(), TileID.SnowBlock);
    }
}
