using System;
using Terraria.Audio;
using Terraria.GameContent;

using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Particles.Projectile;
using ITD.Particles;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
{
    public class CyanideArrow : ModProjectile
    {
		public ParticleEmitter emitter;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(1); // assume wooden arrow identity
			
			emitter = ParticleSystem.NewEmitter<CyaniteFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
            emitter.tag = Projectile;
        }
		
		public override void AI()
        {
            if (emitter != null)
                emitter.keptAlive = true;
        }

		public override void OnKill(int timeLeft)
        {
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
			emitter?.Emit(Projectile.Center, new Vector2(), 2f, 20);
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			target.AddBuff(BuffID.Frostburn2, 600);
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile spike = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Projectile.velocity, ModContent.ProjectileType<CyaniteSpike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, Main.rand.NextFloat(0.8f, 1f), 0f);
				spike.localNPCImmunity[target.whoAmI] = -1; // no double hitsies
			}
        }
		
		public override bool OnTileCollide(Vector2 oldVelocity) 
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + oldVelocity, oldVelocity, ModContent.ProjectileType<CyaniteSpike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, Main.rand.NextFloat(0.8f, 1f), 0f);
			}
			return true;
		}
		
		public override bool PreDraw(ref Color lightColor)
        {
			Vector2 position = Projectile.Center - Main.screenPosition;
			
			Texture2D texture = TextureAssets.Extra[98].Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			Vector2 origin = sourceRectangle.Size() / 2f;

			Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50), Main.GlobalTimeWrappedHourly*2f, origin, 1f*Main.essScale, SpriteEffects.None, 0f);
			Main.EntitySpriteDraw(texture, position, sourceRectangle, new Color(120, 184, 255, 50), Main.GlobalTimeWrappedHourly*2f+MathHelper.PiOver2, origin, 1f*Main.essScale, SpriteEffects.None, 0f);

            return true;
        }
    }
}