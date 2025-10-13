using ITD.Utilities;

namespace ITD.Content.Items.Accessories.Combat.Melee.Snaptraps;

public class Winch : ModItem
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
        var plr = player.GetSnaptrapPlayer();
        plr.LengthModifier += 0.1f;
        plr.RetractVelocityModifier += 0.15f;
    }
}