using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.DataStructures;
using ITD.Content.Tiles.Misc;
using ITD.Content.Dusts;
using Terraria.GameContent.Drawing;
using Terraria.Audio;
using Mono.Cecil;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class ReachingHand : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.height = 78;
            Projectile.width = 70;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.98f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.alpha += 5;

            if (Projectile.alpha > 254)
            {
                Projectile.Kill();
            }

            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Clentaminator_Blue, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            if (player.direction == -1)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
                for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
                {
                    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.velocity.ToRotation() + MathHelper.PiOver2, drawOrigin, Projectile.scale, SpriteEffects.FlipHorizontally, 0);
                }

                return true;
            }
            else
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
                for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
                {
                    Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                    Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.velocity.ToRotation() + MathHelper.PiOver2, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
                }

                return true;
            }
        }
    }
}