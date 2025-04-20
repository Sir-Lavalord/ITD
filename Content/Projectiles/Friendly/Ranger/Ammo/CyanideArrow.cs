using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

using ITD.Particles.Projectile;
using ITD.Particles;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
{
    public class CyanideArrow : ModProjectile
    {
		private bool explosion = false;

		public ParticleEmitter emitter;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(1); // assume wooden arrow identity
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			
			emitter = ParticleSystem.NewEmitter<CyaniteFlash>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
            emitter.tag = Projectile;
        }
		
		public override void AI()
        {
            if (emitter != null)
                emitter.keptAlive = true;
        }

		public override void Kill(int timeLeft)
        {
			Projectile.penetrate = -1;
			Projectile.Resize(100, 100);
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
			emitter?.Emit(Projectile.Center, new Vector2(), 2f, 20);
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 600);
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