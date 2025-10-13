using ITD.Content.Buffs.EquipmentBuffs;

namespace ITD.Content.Items.Accessories.Misc;

internal class CrimsonAntidote : ModItem
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
        player.GetModPlayer<CrimsonAntidotePlayer>().hasCrimsonAntidote = true;
    }
}

internal class CrimsonAntidotePlayer : ModPlayer
{
    public bool hasCrimsonAntidote;
    public bool shortSickness = false;
    public float buffTimeRemaining;

    public override void ResetEffects()
    {
        hasCrimsonAntidote = false;
    }

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        if (hasCrimsonAntidote)
        {
            healValue = (int)(healValue * 1.2f);
        }
    }

    public override void PreUpdate()
    {
        if (hasCrimsonAntidote)
        {
            if (!shortSickness && Player.potionDelay > 0)
            {
                buffTimeRemaining = Player.potionDelay;
                Player.potionDelay = (int)(buffTimeRemaining * 0.85f);
                Player.ClearBuff(BuffID.PotionSickness);
                Player.AddBuff(BuffID.PotionSickness, Player.potionDelay);
                shortSickness = true;
            }
        }
        else
        {
            if (shortSickness && Player.potionDelay > 0)
            {
                buffTimeRemaining = Player.potionDelay;
                Player.potionDelay = (int)(buffTimeRemaining / 85 * 100);
                Player.ClearBuff(BuffID.PotionSickness);
                Player.AddBuff(BuffID.PotionSickness, Player.potionDelay);
                shortSickness = false;
            }
        }
        if (!Player.HasBuff(BuffID.PotionSickness))
        {
            shortSickness = false;
        }
    }
}

internal class CrimsonAntidoteHealItem : GlobalItem
{
    public override bool? UseItem(Item item, Player player)
    {
        if (item.healLife > 0 && player.GetModPlayer<CrimsonAntidotePlayer>().hasCrimsonAntidote)
        {
            player.AddBuff(ModContent.BuffType<CrimsonAntidoteBuff>(), 60 * 5, false);
        }
        return null;
    }
}
