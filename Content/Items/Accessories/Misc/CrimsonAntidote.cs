using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Security.Cryptography.Pkcs;
using System.Net.Http.Headers;
using System;

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
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CrimsonAntidotePlayer>().hasCrimsonAntidote = true;
        }
    }

    internal class CrimsonAntidotePlayer : ModPlayer
    {
        public bool hasCrimsonAntidote;
        public int RegBoostTimer;
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

        public override void UpdateLifeRegen()
        {
            if (hasCrimsonAntidote && RegBoostTimer > 0)
            {
                Player.lifeRegen += 5;
                RegBoostTimer--;
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
            if (item.healLife > 0)
            {
                player.GetModPlayer<CrimsonAntidotePlayer>().RegBoostTimer = 60 * 4;
            }
            return true;
        }
    }
}
