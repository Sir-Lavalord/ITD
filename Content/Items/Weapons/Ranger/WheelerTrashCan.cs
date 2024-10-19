using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using ITD.Content.Projectiles;
using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class WheelerTrashCan : ModItem
    {

        public override void SetDefaults()
        {
            Item.damage = 40;
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
            Item.shootSpeed = 16f;
            Item.autoReuse = true;
        }
        Vector2 distance;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            const float gravity = 0.2f;
            float time = 60f;
            distance =  Main.MouseWorld - player.Center;
            distance.X = distance.X / time;
            distance.Y = distance.Y / time - 0.5f * gravity * time;
            velocity = distance;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 4; i++)
            {
                SoundEngine.PlaySound(SoundID.Item61, player.Center);
                Projectile proj = Projectile.NewProjectileDirect(source, position, distance, type, damage, knockback, player.whoAmI);
            }
            for (int k = 0; k < 16; k++)
            {
                int dust = Dust.NewDust(position - new Vector2(6, 6), 12, 12, DustID.Smoke, 0f, 0f, 0, default, 2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = velocity * Main.rand.NextFloat(1.5f);
            }
            return false;
        }
    }
}
