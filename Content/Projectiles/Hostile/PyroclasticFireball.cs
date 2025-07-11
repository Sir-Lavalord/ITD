using ITD.Particles;
using ITD.Particles.Misc;

namespace ITD.Content.Projectiles.Hostile
{
    public class PyroclasticFireball : ModProjectile
    {
        public override string Texture => ITD.BlankTexture;
        public ParticleEmitter emitter;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.width = Projectile.height = 24;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            if (!Main.dedServ)
            {
                if (emitter is null)
                {
                    emitter = ParticleSystem.NewEmitter<PyroclasticParticle>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
                    emitter.additive = true;
                }
                emitter.keptAlive = true;
                emitter?.Emit(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 3, Projectile.height / 3), -Projectile.velocity / 32f);
            }
            Lighting.AddLight(Projectile.Center, Color.Orange.ToVector3());
        }
        public override void OnKill(int timeLeft)
        {
            emitter = null;
        }
    }
}
