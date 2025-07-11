using System;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Ranger
{
    public class SkyshooterFallingStar : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = 22; Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 500;
			
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
        }

		public override Color? GetAlpha(Color lightColor)
        {
			Color color = new Color(255, 255, 255, 255);
			
            return color * Math.Min(1f, Projectile.localAI[0]*0.1f);
        }

        public override void AI()
        {
			if (Projectile.localAI[0] == 0f)
			{
                Projectile.scale = 2f * Projectile.ai[0];
				Projectile.localAI[0]++;
			}
			Projectile.localAI[0] *= 1f + (0.075f * Projectile.ai[0]);
			
            Projectile.rotation += 0.05f;
            Projectile.velocity = new Vector2(Projectile.ai[1], Projectile.ai[2]) * Projectile.localAI[0];

            //Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<StarDust>(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = Projectile.oldPos.Length - 1; k > 0; k--)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}