using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Security.Cryptography.Pkcs;

namespace ITD.Content.Items.Accessories.Misc
{
    internal class CrimsonAntidote : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.sellPrice(gold: 3);

            Item.accessory = true;
        }

        public float buffTimeRemaining;
        public float actualMaxBuffLenght;
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CrimsonAntidotePlayer>().hasCrimsonAntidote = true;
            player.potionDelayTime = (int)(player.potionDelayTime * 0.85f);
            if (player.HasBuff(BuffID.PotionSickness))
            {
                buffTimeRemaining = player.potionDelay;
                if (player.potionDelay > player.potionDelayTime)
                {
                    player.ClearBuff(BuffID.PotionSickness);
                    player.potionDelay = (int)(player.potionDelay * 0.85f);
                    player.AddBuff(BuffID.PotionSickness, (int)(buffTimeRemaining * 0.85f));
                }
                actualMaxBuffLenght = (int)(buffTimeRemaining / 85 * 100);
                if (player.pStone)
                {
                    actualMaxBuffLenght = (int)(buffTimeRemaining / 85 / 75 * 10000);
                }
                player.GetModPlayer<CrimsonAntidotePlayer>().resetToTime = (int)actualMaxBuffLenght;
            }
        }
    }

    internal class CrimsonAntidotePlayer : ModPlayer
    {
        public bool hasCrimsonAntidote;
        public int resetToTime;

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

        public override void UpdateLifeRegen()
        {
            if (hasCrimsonAntidote && Player.potionDelay > Player.potionDelayTime - 60 * 4)
            {
                Player.lifeRegen += 5;
            }
        }
        public override void PreUpdate()
        {
            
            if (resetToTime != 0 && !hasCrimsonAntidote)
            {
                Player.ClearBuff(BuffID.PotionSickness);
                Player.potionDelay = (int)(resetToTime);
                Player.AddBuff(BuffID.PotionSickness, (int)(resetToTime));
                resetToTime = 0;
            }
        }
    }
}
