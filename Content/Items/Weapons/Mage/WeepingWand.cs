using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

using ITD.Content.Items.Placeable;
using ITD.Utilities;
using ITD.Content.Projectiles.Friendly.Mage;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Content.Projectiles.Friendly;
using System;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;


namespace ITD.Content.Items.Weapons.Mage
{
    public class WeepingWand : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.autoReuse = true;

            Item.DamageType = DamageClass.Magic;
            Item.damage = 8;
            Item.knockBack = 1f;
            Item.crit = 4;
            Item.noMelee = true;
            Item.mana = 6;

            Item.value = Item.buyPrice(silver: 25);
            Item.rare = 2;
            Item.UseSound = SoundID.Item71;
            Item.shoot = ModContent.ProjectileType<WeepWandWisp>();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<Dusts.WispDust>());
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mousePosition = Main.MouseWorld;
            Vector2 direction = mousePosition - player.position;
            direction.Normalize();
            float projectileSpeed = 4f;

            Projectile.NewProjectile(player.GetSource_FromThis(), player.position.X, player.position.Y - 20f, direction.X * projectileSpeed, direction.Y * projectileSpeed, ModContent.ProjectileType<WeepWandWisp>(), 7, 0f, player.whoAmI);
            Projectile.NewProjectile(player.GetSource_FromThis(), player.position.X, player.position.Y, direction.X * (projectileSpeed * 2f), direction.Y * projectileSpeed, ModContent.ProjectileType<WeepWandWisp>(), 7, 0f, player.whoAmI);
            Projectile.NewProjectile(player.GetSource_FromThis(), player.position.X, player.position.Y + 20f, direction.X * projectileSpeed, direction.Y * projectileSpeed, ModContent.ProjectileType<WeepWandWisp>(), 7, 0f, player.whoAmI);

            return false;
        }
    }
}