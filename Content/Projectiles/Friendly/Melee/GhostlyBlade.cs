using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class GhostlyBlade : ModProjectile
    {
		public VertexStrip TrailStrip = new VertexStrip();
		
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 60; Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
        }
		
		public override Color? GetAlpha(Color lightColor)
        {
			Color color = new Color(255, 255, 255, 100);
            return color * Projectile.Opacity;
        }
		
		public override void OnSpawn(IEntitySource source)
		{
			SoundEngine.PlaySound(SoundID.NPCHit54, Projectile.position);
			for (int j = 0; j < 8; ++j)
			{
				int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.DungeonSpirit, 0f, 0f, 100, default(Color), 1.5f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 3f;
			}
		}

        public override void AI()
        {
			if (Projectile.timeLeft > 10)
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.DungeonSpirit, 0, 0, 100, default, 1f);
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
			Color result = Color.Lerp(Color.White, new Color(9, 157, 255), Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
			result.A /= 2;
			return result * Projectile.Opacity * Projectile.Opacity;
		}
		private float StripWidth(float progressOnStrip)
		{
			return MathHelper.Lerp(36f, 48f, Utils.GetLerpValue(0f, 0.2f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			GameShaders.Misc["MagicMissile"].Apply(null);
            TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
						
			return true;
		}
    }
}
