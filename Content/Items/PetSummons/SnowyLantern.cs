using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Pets;
using ITD.Content.Buffs.PetBuffs;
using Microsoft.Xna.Framework;

namespace ITD.Content.Items.PetSummons
{
    public class SnowyLantern : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item2;
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.rare = ItemRarityID.Yellow;
            Item.noMelee = true;
            Item.height = 32;
            Item.width = 32;
            Item.shoot = ModContent.ProjectileType<WingedSnowpoffPet>();
            Item.buffType = ModContent.BuffType<SnowyLanternBuff>();
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(Item.buffType, 3600);
            }
        }
    }
}
