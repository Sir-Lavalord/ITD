using ITD.Content.Buffs.EquipmentBuffs;

namespace ITD.Content.Items.Accessories.Misc;

internal class CorruptAntidote : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.sellPrice(gold: 3);

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<CorruptAntidotePlayer>().hasCorruptAntidote = true;
        player.manaSickReduction *= 0.85f;

    }
}

internal class CorruptAntidotePlayer : ModPlayer
{
    public bool hasCorruptAntidote;

    public override void ResetEffects()
    {
        hasCorruptAntidote = false;
    }

    public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
    {
        if (hasCorruptAntidote)
        {
            healValue = (int)(healValue * 1.2f);
        }
    }
}

internal class CorruptAntidoteManaItem : GlobalItem
{

    public override bool? UseItem(Item item, Player player)
    {
        if (item.healMana > 0 && player.GetModPlayer<CorruptAntidotePlayer>().hasCorruptAntidote)
        {
            player.AddBuff<CorruptAntidoteBuff>(60 * 5, false);
        }
        return null;
    }
}
