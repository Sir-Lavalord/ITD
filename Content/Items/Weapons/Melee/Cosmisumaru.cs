using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using ITD.Players;
using ITD.Systems;
using Terraria.DataStructures;
using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Misc;
using Terraria.Audio;
using ITD.Content.Projectiles.Friendly.Melee;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ITD.Content.Dusts;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Linq.Expressions;
using ReLogic.Graphics;
using static System.Net.Mime.MediaTypeNames;
using ITD.Content.Items.Favors;

namespace ITD.Content.Items.Weapons.Melee
{
    public class Cosmisumaru : ModItem
    {
        public int attackCycle = 0;
        public Vector2 dustPosition;
        public override void SetDefaults()
        {
            Item.mana = 0;
            Item.damage = 160;
            Item.crit = 4;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 114;
            Item.useTime = 2;
            Item.useAnimation = 60;
            Item.channel = true;
            Item.noMelee = true;
            Item.useStyle = 6;
            Item.knockBack = 6;
            Item.value = Item.buyPrice(0, 22, 50, 0);
            Item.rare = 9;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<CosmisumaruCut>();
            Item.shootSpeed = 40f;
            Item.scale = 0.65f;
            Item.holdStyle = -1;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public static void Channel(Item item)
        {
            item.channel = false;
        }
     
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(0, 0);
        }

        public override void HoldItem(Player player)
        {

            if (player.direction < 0)
            {
                player.itemRotation =  5f;
                player.itemLocation.Y = player.Center.Y + 10f;
                player.itemLocation.X = player.Center.X + 40f;
            }
            else if (player.direction > 0)
            {
                player.itemRotation =  -5f;
                player.itemLocation.Y = player.Center.Y + 10f;
                player.itemLocation.X = player.Center.X - 40f;
            }

            if (player.GetITDPlayer().charge > 39)
            {
                if (Main.rand.NextBool(3))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<CosmisumaruIndicator>(), 0f, 0f, 150, default(Color), 1.5f);
                    }
                }
            }
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.direction < 0)
            {
                player.itemRotation =  5f;
                player.itemLocation.Y = player.Center.Y + 10f;
                player.itemLocation.X = player.Center.X + 40f;
            }
            else if (player.direction > 0)
            {
                player.itemRotation =  -5f;
                player.itemLocation.Y = player.Center.Y + 10f;
                player.itemLocation.X = player.Center.X - 40f;
            }

            if (player.GetITDPlayer().charge > 39)
            {
                if (Main.rand.NextBool(3))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<CosmisumaruIndicator>(), 0f, 0f, 150, default(Color), 1.5f);
                    }
                }
            }
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.GetITDPlayer().charge < 39)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            } 
            else
            {
                return true;
            }
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.GetITDPlayer().charge > 39)
                {
                    player.GetITDPlayer().charge = 0;
                    Vector2 mousePosition = Main.MouseWorld;
                    Vector2 direction = mousePosition - player.position;
                    direction.Normalize();
                    float projectileSpeed = 8f;

                    Projectile.NewProjectile(player.GetSource_FromThis(), player.position.X, player.position.Y, direction.X * projectileSpeed, direction.Y * projectileSpeed, ModContent.ProjectileType<CosmisumaruPheonix>(), 360, 0f, player.whoAmI);

                    for (int d = 0; d < 30; d++)
                    {
                        Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<CosJelDust>(), 0f, 0f, 150, default(Color), 1.5f);
                    }
                    
                    return true;
                } 
                else
                {
                    return false;
                }
            }
            else
            {
                Item.damage = 100;
                Item.crit = 4;
                Item.knockBack = 6;
                Item.shoot = ModContent.ProjectileType<CosmisumaruCut>();

                return true;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2 && player.GetITDPlayer().charge > 39)
            {
                position.Y = Main.MouseWorld.Y;
                position.X = Main.MouseWorld.X;
                velocity.X = 0f;
                velocity.Y = 0f;
                Item.shoot = ModContent.ProjectileType<CosmisumaruPheonix>();
                return true;
            }

            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Main.GetItemDrawFrame(Item.type, out var itemTexture, out var itemFrame); 
            Vector2 drawOrigin = itemFrame.Size() / 2f; 
            Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, drawOrigin.Y);
            scale *= 0.5f;
            Color drawColor = Item.GetAlpha(alphaColor);
            Texture2D texture = ModContent.Request<Texture2D>("ITD/Content/Items/Weapons/Melee/Cosmisumaru").Value;
            spriteBatch.Draw(texture, drawPosition, null, drawColor, rotation, texture.Size() / 2f, scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
