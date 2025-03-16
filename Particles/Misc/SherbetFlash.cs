using ITD.Utilities.EntityAnim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Particles.Misc
{
    public class SherbetFlash : ParticleEmitter
    {
        /*
        public override void SetDefaults()
        {
            canvas = ParticleEmitterDrawCanvas.WorldUnderProjectiles;
            scale *= 1.6f;
            timeLeft = 40;
        }
        */
        public override void SetStaticDefaults()
        {
            ParticleSystem.particleUsesRenderTarget[type] = true;
        }
        public override void OnEmitParticle(ref ITDParticle particle)
        {
			particle.scale = 0f;
        }
		public override Color GetAlpha(ITDParticle particle)
        {
            return new Color(255, 73, 146, 0) * (particle.ProgressOneToZero * 6f);
        }
		public override void AI(ref ITDParticle particle)
        {
            particle.scale = EasingFunctions.OutQuart(particle.ProgressZeroToOne) * particle.rotation;
        }
    }
}
