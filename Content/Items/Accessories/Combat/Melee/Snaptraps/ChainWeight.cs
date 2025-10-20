using ITD.Systems;

namespace ITD.Content.Items.Accessories.Combat.Melee.Snaptraps;

public class ChainWeight : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 28;
        Item.value = Item.buyPrice(10);
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<SnaptrapPlayer>().ChainWeightEquipped = true;
    }
}