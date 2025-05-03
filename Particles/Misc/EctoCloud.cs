using ITD.Content.Items.Dyes;
using ITD.Content.Projectiles.Unused;
using ITD.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Particles.Misc
{
    public class EctoCloud : ParticleEmitter
    {
        /*
        public override void SetDefaults()
        {
            canvas = ParticleEmitterDrawCanvas.WorldUnderProjectiles;
            scale *= 1.3f;
            timeLeft = 60;
        }
        */
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale *= 2f;
            // timeleft is now set on Emit() call. canvas is set on Emitter initialization.
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.scale = particle.spawnParameters.Scale * particle.ProgressOneToZero;
            particle.velocity *= 0.95f;
        }
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void DrawAllParticles()
        {
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Particles/Textures/EctoCloud").Value;
            Color color1 = new(9, 121, 255);
            Color color2 = new(0, 220, 255);
			Color color3 = new(255, 255, 255);
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawCommon(Main.spriteBatch, tex, CanvasOffset, color1);
            }
			for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawCommon(Main.spriteBatch, tex, CanvasOffset, color2, null, null, null, particles[i].scale * 0.8f);
            }
			for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawCommon(Main.spriteBatch, tex, CanvasOffset, color3, null, null, null, particles[i].scale * 0.6f);
            }
        }
    }
}
