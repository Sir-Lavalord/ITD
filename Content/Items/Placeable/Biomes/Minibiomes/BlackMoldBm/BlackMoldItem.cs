using ITD.Content.Tiles.Misc;

namespace ITD.Content.Items.Placeable.Biomes.Minibiomes.BlackMoldBm;

public class BlackMoldItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }
    public override void SetDefaults()
    {
        Item.DefaultToSeeds();
    }
    public override bool? UseItem(Player player) => Helpers.UseItemPlaceSeeds(player, ModContent.TileType<BlackMold>(), TileID.Stone);
}
