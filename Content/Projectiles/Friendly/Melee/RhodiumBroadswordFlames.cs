using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class RhodiumBroadswordFlames : ModProjectile
    {
		public override string Texture => ITD.BlankTexture;
		
		public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);
		public VertexStrip TrailStrip = new VertexStrip();
		
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 40; Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
			
			Shader.UseImage0("Images/Extra_" + 191);
			Shader.UseImage1("Images/Extra_" + 194);
			Shader.UseImage2("Images/Extra_" + 190);
			Shader.UseSaturation(-2.8f);
			Shader.UseOpacity(2f);
        }
		
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(BuffID.OnFire, 120, false);
        }
		
		public override void OnSpawn(IEntitySource source)
		{
			SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
			for (int j = 0; j < 4; ++j)
			{
				int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch, 0f, 0f, 100, default(Color), 3f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 4f;
			}
		}
		
        public override void AI()
        {
			if (Projectile.timeLeft <= 10)
			{				
				Projectile.velocity *= 0.8f;
				Projectile.Opacity -= 0.1f;
			}
			
			Projectile.rotation = Projectile.velocity.ToRotation();
        }

		private Color StripColors(float progressOnStrip)
		{
			Color result = Color.Lerp(Color.White, new Color(255, 50, 0), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
			result.A /= 2;
			return result * Projectile.Opacity;
		}
		private float StripWidth(float progressOnStrip)
		{
			return 160f;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Shader.Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
						
			return false;
		}
    }
}
