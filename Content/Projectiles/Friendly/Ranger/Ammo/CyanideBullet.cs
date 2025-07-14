using Terraria.Audio;

using ITD.Particles.Projectile;
using ITD.Particles;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
{
    public class CyanideBullet : ModProjectile
    {		
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

            AIType = ProjectileID.Bullet;
			
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
			emitter?.Emit(Projectile.Center, new Vector2(), 1.6f, 20);
        }

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			target.AddBuff(BuffID.Frostburn2, 600);
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile spike = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Projectile.velocity, ModContent.ProjectileType<CyaniteSpike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, Main.rand.NextFloat(0.7f, 0.8f), 0f);
				spike.localNPCImmunity[target.whoAmI] = -1; // no double hitsies
			}
        }
		
		public override bool OnTileCollide(Vector2 oldVelocity) 
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + oldVelocity, oldVelocity, ModContent.ProjectileType<CyaniteSpike>(), Projectile.damage, Projectile.knockBack, Projectile.owner, 0f, Main.rand.NextFloat(0.7f, 0.8f), 0f);
			}
			return true;
		}
    }
}