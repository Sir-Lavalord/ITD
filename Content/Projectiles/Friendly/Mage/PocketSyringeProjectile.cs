using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class PocketSyringeProjectile : ModProjectile
    {
		public VertexStrip TrailStrip = new VertexStrip();
		public ref float Duration => ref Projectile.localAI[0];
		public ref float Stuck => ref Projectile.ai[0];
		public ref float OffsetX => ref Projectile.ai[1];
		public ref float OffsetY => ref Projectile.ai[2];
		
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
			Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

		public override void OnSpawn(IEntitySource source)
		{
			Stuck = -1f;
		}

        public override void AI()
        {
			if (Stuck < 0)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
				
				Duration++;
				if (Duration > 24f)
				{
					Projectile.velocity.X *= 0.97f;
					Projectile.velocity.Y = Projectile.velocity.Y + 0.5f;; 
					
					if (Projectile.velocity.Y > 16f) 
					{
						Projectile.velocity.Y = 16f;
					}
				}
			}
			else
			{
				NPC npc = Main.npc[(int)(Stuck)];
				if (!npc.active)
					Projectile.Kill();
				Projectile.Center = npc.Center + new Vector2(OffsetX, OffsetY);
			}
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Glass);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 0, default, 1.5f);
            }
        }

		private bool CanDebuffEnemies(int buffType)
        {
            switch (buffType)
            {
                case BuffID.Venom:
                case BuffID.Bleeding:
                case BuffID.Confused:
                case BuffID.CursedInferno:
                case BuffID.Frostburn:
                case BuffID.Frostburn2:
                case BuffID.OnFire:
                case BuffID.OnFire3:
                case BuffID.Ichor:
                case BuffID.Poisoned:
                case BuffID.ShadowFlame:
                case BuffID.Slimed:
                case BuffID.Stinky:
                case BuffID.Wet:
                    return true;
                default:
                    return false;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.active)
            {
                for (int i = 0; i < 20; i++)
                {
                    if (player.buffType[i] > 0)
                    {
                        int buffType = player.buffType[i];
                        int buffTime = player.buffTime[i];
						if (CanDebuffEnemies(buffType))
						{
							if (buffType == BuffID.Bleeding)
								buffType = ModContent.BuffType<BleedingII>();
							target.AddBuff(buffType, buffTime);
						}
                    }
                }
            }
			
			Stuck = target.whoAmI;
			OffsetX = Projectile.Center.X - target.Center.X;
			OffsetY = Projectile.Center.Y - target.Center.Y;
			Projectile.timeLeft = 120;
        }
		
		private Color StripColors(float progressOnStrip)
		{
			Color result = Color.Lerp(Color.White, Color.Red, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, false));
			result.A /= 2;
			return result * 0.5f;
		}
		private float StripWidth(float progressOnStrip)
		{
			return MathHelper.Lerp(8f, 6f, Utils.GetLerpValue(0f, 0.2f, progressOnStrip, true)) * Utils.GetLerpValue(0f, 0.07f, progressOnStrip, true);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			if (Stuck < 0)
			{
				GameShaders.Misc["LightDisc"].Apply(null);
				TrailStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, StripColors, StripWidth, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
				TrailStrip.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}
			
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle rectangle = texture.Frame(1, 1);
			Vector2 position = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, position, rectangle, lightColor, Projectile.rotation, rectangle.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
    }
}