using ITD.Systems;
using ITD.Utilities.Placeholders;

namespace ITD.Content.Items.Accessories.Combat.All;

public class SheeringBane : ModItem
{
    public override string Texture => Placeholder.PHAxe;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.DefaultToAccessory(28, 38);
        Item.rare = ItemRarityID.LightRed;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<WeaponEnchantmentPlayer>().flamingRazor = true;
        player.GetModPlayer<WeaponEnchantmentPlayer>().poisonFang = true;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<FlamingRazor>(), 1)
            .AddIngredient(ModContent.ItemType<PoisonFang>(), 1)
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
    }
}
