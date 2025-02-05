//might be removed

/*using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ITD.Content.Projectiles;
using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using ITD.Content.Projectiles.Friendly.Ranger;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class WheelerTrashCan : ModItem
    {

        public override void SetDefaults()
        {
            Item.damage = 30;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 30;
            Item.height = 60;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.PewMaticHornShot;
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0f, -6f);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<WheelerTrashProj>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
*//*            if (iCurrentChance >= 98)
            {
                iPicked = 14;
            }
            else if (iCurrentChance >= 98 && iCurrentChance < 98)*//*
            SoundEngine.PlaySound(SoundID.Item36, position);
            for (int i = 0; i < 4; i++)
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(20));
                newVelocity *= 1f - Main.rand.NextFloat(0.3f);
                Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI,Main.rand.Next(0,15));
            }
            Vector2 muzzleOffset = Vector2.Normalize(velocity) * 60f;
            for (int i = 0; i < 16; i++)
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(30));
                newVelocity *= Main.rand.NextFloat(2f);
                int dust = Dust.NewDust(position, Item.width/2, Item.height / 2, DustID.Smoke, 0f, 0f, 0, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = newVelocity;
            }
            position += muzzleOffset;

            return false;
        }
    }
}
*/