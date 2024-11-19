using ITD.Utilities.EntityAnim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Particles.Misc
{
    public class ProtectedTileParticle : ITDParticle
    {
        public override void SetDefaults()
        {
            timeLeft = 30;
            scale = 0.1f;
            additive = true;
        }
        public override void AI()
        {
            scale = spawnScale + (EasingFunctions.OutQuad(ProgressZeroToOne) * 0.5f);
            opacity = ProgressOneToZero;
        }
    }
}
