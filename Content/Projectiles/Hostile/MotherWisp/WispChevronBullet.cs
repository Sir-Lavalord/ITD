using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp
{
    public class WispChevronBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 480;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0) Projectile.alpha -= 20;

            float chevronIndex = Projectile.ai[0];
            Projectile.ai[1]++;
            float time = Projectile.ai[1];

            float formTime = 30f; 
            float morphDuration = 35f;
            float finalUnifiedSpeed = 9f;
            if (time < formTime)
            {
                float minSpeed = 4f;
                float gentleBrake = 0.85f;

                if (Projectile.velocity.Length() > minSpeed)
                {
                    Projectile.velocity *= gentleBrake;
                }
            }
            else if (time < formTime + morphDuration)
            {
                float catchupFactor = 0.25f;
                float speedAdjustment =  (Math.Abs(chevronIndex) * catchupFactor);

                if (chevronIndex == 0) speedAdjustment = 1f;

                float targetSpeed = finalUnifiedSpeed * speedAdjustment;
                Vector2 targetVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * targetSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, 0.08f);
            }
            else
            {
                Vector2 finalVel = Projectile.velocity.SafeNormalize(Vector2.UnitY) * finalUnifiedSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, finalVel, 0.15f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
                Main.EntitySpriteDraw(tex, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}