using System;
using Terraria.Audio;

using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class AccursedSpirit : ModProjectile
    {
		private bool explosion = false;
		public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
        }
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			if (explosion)
				return;
			
			explosion = true;
			
			Projectile.timeLeft = 20;
			Projectile.velocity *= 0f;
			
			Projectile.Resize(100, 100);
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			for (int j = 0; j < 8; ++j)
            {
                int dust = Dust.NewDust(Projectile.Center, 10, 1, DustID.SteampunkSteam, 0f, 0f, 0, default(Color), 1.5f);
				Main.dust[dust].velocity *= 2f;
            }
        }
		
		public override void AI()
        {
            float maxDetectRadius = 750f;

            if (!explosion)
            {				
				Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
				
				int dust = Dust.NewDust(Projectile.position, 30, 30, DustID.SteampunkSteam, 0f, 0f, 0, default(Color), 1f);
				Main.dust[dust].velocity *= 0.2f;
				
				if (++Projectile.frameCounter >= 3)
				{
					Projectile.frameCounter = 0;
					Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
				}
		
                NPC HomingTarget = Projectile.FindClosestNPC(maxDetectRadius);
                if (HomingTarget == null)
                    return;

                float length = Projectile.velocity.Length();
                float targetAngle = Projectile.AngleTo(HomingTarget.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(6)).ToRotationVector2() * length;
                Projectile.Center += Main.rand.NextVector2Circular(1, 1);
            }
        }
		
		public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
        public override bool PreDraw(ref Color lightColor)
        {
			if (explosion)
			{
				Vector2 position = Projectile.Center - Main.screenPosition;
			
				Texture2D texture = ModContent.Request<Texture2D>(Texture + "_Explosion").Value;
				Rectangle sourceRectangle = texture.Frame(1, 1);
				Vector2 origin = sourceRectangle.Size() / 2f;

				
				float scaleMultipler = (20f-Projectile.timeLeft)*0.075f;
				float colorMultiplier = Math.Min(1, Projectile.timeLeft*0.2f);
				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(255, 255, 255, 150)*colorMultiplier, scaleMultipler*2f, origin, scaleMultipler, SpriteEffects.None, 0f);				
				Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(255, 255, 255, 150)*colorMultiplier, scaleMultipler, origin, scaleMultipler*0.75f, SpriteEffects.None, 0f);
				
				return false;
			}
			
            return true;
        }
    }
}
