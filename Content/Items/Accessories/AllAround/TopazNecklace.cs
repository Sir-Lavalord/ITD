namespace ITD.Content.Items.Accessories.AllAround;

public class TopazNecklace : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 34;
        Item.value = Item.sellPrice(50000);
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        // ITDPlayer modPlayer = player.GetModPlayer<ITDPlayer>();
        // Add stuff for lightning here
    }
}
