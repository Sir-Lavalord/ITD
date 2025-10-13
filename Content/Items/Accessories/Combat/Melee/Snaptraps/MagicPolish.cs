using ITD.Utilities;

namespace ITD.Content.Items.Accessories.Combat.Melee.Snaptraps;

public class MagicPolish : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 58;
        Item.height = 22;
        Item.value = Item.buyPrice(10);
        Item.rare = ItemRarityID.Green;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetSnaptrapPlayer().FullPowerHitsModifier.Flat += 1;
        player.GetSnaptrapPlayer().RetractVelocityModifier += 0.05f;
    }
}