using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace ITD.Content.Projectiles.Hostile
{
    public class QuartzBlast : ModProjectile
    {
		public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile")
			.UseProjectionMatrix(true)
			.UseImage0("Images/Extra_" + 190)
			.UseImage1("Images/Extra_" + 190)
			.UseImage2("Images/Extra_" + 197)
			.UseSaturation(-2.8f)
			.UseOpacity(2f);
		public VertexStrip TrailStrip = new VertexStrip();
		
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 25;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
        }
		
		public override Color? GetAlpha(Color lightColor)
        {
			Color color = new Color(255, 255, 255, 100);
            return color * Projectile.Opacity;
        }

        public override void AI()
        {
			if (Projectile.timeLeft > 10)
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GiantCursedSkullBolt, 0, 0, 100, default, 1f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity = Projectile.velocity/5f;
			}
			else
			{				
				if (Projectile.timeLeft == 10)
					Projectile.velocity *= 0.2f;
				Projectile.Opacity -= 0.1f;
			}
			
			Projectile.spriteDirection = (Projectile.velocity.X < 0).ToDirectionInt();
			if (Projectile.spriteDirection == 1)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2*2;
			else
				Projectile.rotation = Projectile.velocity.ToRotation();
        }
				
		private Color StripColors(float progressOnStrip)
		{
			Color result = new Color(255, 132, 255);
			result.A /= 2;
			return result * Projectile.Opacity * Projectile.Opacity;
		}
		private float StripWidth(float progressOnStrip)
        {
            return MathHelper.Lerp(20f, 15f, Utils.GetLerpValue(0f, 0.6f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.1f, progressOnStrip, true) * 1.2f;
        }
		public override bool PreDraw(ref Color lightColor)
		{
			//GameShaders.Misc["MagicMissile"].Apply(null);
			Shader.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			
			return false;
		}
    }
}
