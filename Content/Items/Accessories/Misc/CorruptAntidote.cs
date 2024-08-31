using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ITD.Content.Items.Accessories.Misc
{
    internal class CorruptAntidote : ModItem
    {
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
        public bool consumedManaPotion;
        public int buffTimer;

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

        public override void PreUpdate()
        {
            if (consumedManaPotion && hasCorruptAntidote)
            {
                buffTimer = 60 * 4;
                consumedManaPotion = false;
            }
            else
            {
                consumedManaPotion = false;
            }
        }

        public override void UpdateEquips()
        {
            if (buffTimer > 0)
            {
                Player.GetDamage(DamageClass.Magic) *= 1.25f;
                Player.manaCost *= 0.85f;
                buffTimer--;
            }
        }
    }

    internal class CorruptAntidoteManaItem : GlobalItem
    {

        public override bool ConsumeItem(Item item, Player player)
        {
            if (IsManaItem(item))
            {
                player.GetModPlayer<CorruptAntidotePlayer>().consumedManaPotion = true;
            }
            return true;
        }

        private bool IsManaItem(Item item)
        {
            return item.type == ItemID.LesserManaPotion ||
                   item.type == ItemID.ManaPotion ||
                   item.type == ItemID.GreaterManaPotion ||
                   item.type == ItemID.SuperManaPotion;
        }
    }
}
