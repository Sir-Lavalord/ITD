namespace ITD.Particles.Misc
{
    public class BloodParticle : ParticleEmitter
    {
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale *= 1.3f;
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.scale = particle.spawnParameters.Scale * particle.ProgressOneToZero;
            particle.velocity *= 0.9f;
        }
    }
}
