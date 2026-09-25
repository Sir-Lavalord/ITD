using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Hostile.MotherWisp
{
    public class WispScythe : ModProjectile
    {
        public override string Texture => "ITD/Content/Projectiles/Hostile/MotherWisp/WispFireBall";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 15;
            }

            Projectile.ai[0]++;

            Projectile.rotation += 0.4f * (Projectile.velocity.X > 0 ? 1f : -1f);

            if (Projectile.ai[0] > 24f + Projectile.ai[1])
            {
                float maxSpeed = 18f;

                if (Projectile.velocity.Length() < maxSpeed)
                {
                    Projectile.velocity *= 1.08f;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.6f, 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(tex, drawPos, null, color * 0.5f, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}