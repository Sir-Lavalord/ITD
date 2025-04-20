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
    public class CyanideBullet : ModProjectile
    {
		private bool explosion = false;
		
		public ParticleEmitter emitter;
		
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;

            AIType = ProjectileID.Bullet;
			
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
			Projectile.Resize(75, 75);
			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
			emitter?.Emit(Projectile.Center, new Vector2(), 1.6f, 20);
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 600);
		}
    }
}