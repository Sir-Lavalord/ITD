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

namespace ITD.Particles.CosJel
{
    public class SpaceMist : ParticleEmitter
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
            particle.scale *= 1.3f;
            // timeleft is now set on Emit() call. canvas is set on Emitter initialization.
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.scale = particle.spawnParameters.Scale * particle.ProgressOneToZero;
            particle.velocity *= 0.95f;
        }
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void PreDrawAllParticles()
        {
            Texture2D tex = ModContent.Request<Texture2D>("ITD/Particles/Textures/SpaceMist_Outline").Value;
            Color color1 = new(255, 242, 191);
            Color color2 = new(168, 241, 255);
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].DrawCommon(Main.spriteBatch, tex, CanvasOffset, Color.Lerp(color1, color2, Utils.PingPongFrom01To010(MathHelper.Lerp(0, 1, timeLeft / 60f))));
            }
        }
    }
}
