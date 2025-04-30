using ITD.Utilities.EntityAnim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
