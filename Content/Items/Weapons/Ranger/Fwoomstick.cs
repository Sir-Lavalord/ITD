using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;

using ITD.Systems;
using ITD.Players;
using ITD.Utilities;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Text;
using System.IO;
using ITD.Content.Projectiles.Other;
using System.Collections.Generic;
using System;
using Terraria.GameContent.Creative;
using Terraria.Localization;
using ITD.Content.Projectiles;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class Fwoomstick : ModItem
    {


        public override void SetStaticDefaults()
        {
            FrontGunLayer.RegisterData(Item.type);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 100;
            Item.height = 20;
            Item.useTime = 55;
            Item.useAnimation = 55;
            Item.useStyle = -1;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(silver: 40);
            Item.rare = ItemRarityID.Green;
            Item.shootSpeed = 4f;
            Item.useAmmo = AmmoID.Bullet;
            Item.autoReuse = false;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileID.PurificationPowder;

            Item.scale = 0.75f;
        }

        public void Hold(Player player)
        {
            ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

            if (mouse.X < player.Center.X)
                player.direction = -1;
            else
                player.direction = 1;

            float rotation = (Vector2.Normalize(mouse - player.MountedCenter)*player.direction).ToRotation();
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, -20f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            const int NumProjectiles = 8;

            for (int i = 0; i < NumProjectiles; i++)
            {

                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15));

                newVelocity *= 1f - Main.rand.NextFloat(0.3f);

                int proj = Projectile.NewProjectile(source, position, newVelocity, type, damage, knockback, player.whoAmI);
                Main.projectile[proj].GetGlobalProjectile<PotshotBullet>().isFromFwoomstick = true;
            }

            return false;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = velocity.SafeNormalize(Vector2.Zero) * 116f;

            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }
    }
}