using ITD.Systems;
using ITD.Utilities;

namespace ITD.Content.Items.Accessories.Combat.All;

public class SoulTalisman : ModItem
{
    /*        public override string Texture => Placeholder.PHAxe;*/

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.DefaultToAccessory(28, 38);
        Item.rare = ItemRarityID.Yellow;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        ITDPlayer itdPlayer = player.GetITDPlayer();
        itdPlayer.soulTalisman = true;
    }
    public override Color? GetAlpha(Color lightColor)
    {
        return Color.White;
    }
}
