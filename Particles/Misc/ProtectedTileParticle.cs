using ITD.Utilities.EntityAnim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Particles.Misc
{
    public class ProtectedTileParticle : ParticleEmitter
    {
        /*
        public override void SetDefaults()
        {
            timeLeft = 30;
            scale = 0.1f;
            additive = true;
        }
        */
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale = 0.1f;
            // timeleft is now set on Emit() call. canvas is set on Emitter initialization.
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.scale = particle.spawnParameters.Scale + (EasingFunctions.OutQuad(particle.ProgressZeroToOne) * 0.5f);
            particle.opacity = particle.ProgressOneToZero;
        }
    }
}
