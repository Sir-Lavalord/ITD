namespace ITD.Content.Items.Armor.Cyanite;

[AutoloadEquip(EquipType.Body)]
public class CyanitePlating : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 22;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Green;
        Item.defense = 22;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Generic) += 0.05f;
        //+5% Damage reduction somehow
    }
}
