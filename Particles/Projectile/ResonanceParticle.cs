using ITD.Utilities.EntityAnim;

namespace ITD.Particles.Projectile
{
    public class ResonanceParticle : ParticleEmitter
    {
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale = 0.8f;
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.scale = particle.spawnParameters.Scale + (EasingFunctions.OutQuad(particle.ProgressZeroToOne) * 0.5f);
            particle.opacity = particle.ProgressOneToZero;
        }
        public override Color GetAlpha(ITDParticle particle)
        {
            return new Color(243, 162, 63);
        }
    }
}
