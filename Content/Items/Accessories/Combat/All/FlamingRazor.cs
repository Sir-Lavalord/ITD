using ITD.Utilities.Placeholders;
using ITD.Systems;

namespace ITD.Content.Items.Accessories.Combat.All
{
    public class FlamingRazor : ModItem
    {
        public override string Texture => Placeholder.PHAxe;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(28, 38);
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WeaponEnchantmentPlayer>().flamingRazor = true;
        }
    }
}
