using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
{
    public class IncendiaryShot : ModProjectile
    {
		public override string Texture => ITD.BlankTexture;
		
		public MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);
		public VertexStrip TrailStrip = new VertexStrip();
		
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.alpha = 255;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
			Projectile.timeLeft = 40;
			
			AIType = ProjectileID.Bullet;
			
			Shader.UseImage0("Images/Extra_" + 191);
			Shader.UseImage1("Images/Extra_" + 194);
			Shader.UseImage2("Images/Extra_" + 190);
			Shader.UseSaturation(-2.8f);
			Shader.UseOpacity(2f);
        }
		
		public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
		
		public override void OnKill(int timeLeft)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 3; i++)
                {
                    float speedX = Main.rand.NextFloat(-4f, 4f) + Projectile.velocity.X;//Projectile.velocity.X * Main.rand.NextFloat(.4f, .7f) + Main.rand.NextFloat(-2f, 2f);
                    float speedY = Main.rand.NextFloat(-4f, 4f) + Projectile.velocity.Y;//Projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, speedX, speedY, ModContent.ProjectileType<FwoomstickSpark>(), (int)(Projectile.damage * 0.5), 0f, Projectile.owner, 0f, 0f);
                }
            }
			for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Torch, 0f, 0f, 0, default, 2f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity -= Projectile.velocity * Main.rand.NextFloat(1f);
            }
            SoundEngine.PlaySound(SoundID.Item45, Projectile.position);
        }
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 60);
        }
		
		private Color StripColors(float progressOnStrip)
		{
			Color result = Color.Lerp(Color.White, new Color(255, 50, 0), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
			result.A /= 2;
			return result * Projectile.Opacity;
		}
		private float StripWidth(float progressOnStrip)
		{
			return 24f;
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