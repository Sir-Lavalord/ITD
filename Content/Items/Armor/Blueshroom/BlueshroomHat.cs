using Terraria.Localization;

namespace ITD.Content.Items.Armor.Blueshroom;

[AutoloadEquip(EquipType.Head)]
public class BlueshroomHat : ModItem
{
    public static LocalizedText SetBonusText { get; private set; }
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        SetBonusText = this.GetLocalization("SetBonus");
    }
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 26;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Green;
        Item.defense = 3;
    }
    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<BlueshroomCoat>() && legs.type == ModContent.ItemType<BlueshroomPants>();
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Magic) += 0.07f;
        player.manaCost -= 0.05f;
    }

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = SetBonusText.Value;

        player.buffImmune[BuffID.Slow] = true;
        player.buffImmune[BuffID.Chilled] = true;
    }
}
