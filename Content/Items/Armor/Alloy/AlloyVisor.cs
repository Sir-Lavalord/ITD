using Terraria.Localization;

namespace ITD.Content.Items.Armor.Alloy;

[AutoloadEquip(EquipType.Head)]
public class AlloyVisor : ModItem
{
    public static LocalizedText SetBonusText { get; private set; }
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        SetBonusText = this.GetLocalization("SetBonus");
    }
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 18;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Green;
        Item.defense = 2;
    }
    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<AlloyChestplate>() && legs.type == ModContent.ItemType<AlloyLeggings>();
    }
    public override void UpdateArmorSet(Player player)
    {
        player.ITD().setAlloy_Magic = true;
        player.setBonus = SetBonusText.Value;
    }
}
