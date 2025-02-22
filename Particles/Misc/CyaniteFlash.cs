using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.GameContent;

using ITD.Utilities;

namespace ITD.Particles.Misc
{
    public class CyaniteFlash : ParticleEmitter
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
			particle.scale *= particle.rotation; // hack solution for emitting differently scaled particles
        }
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void DrawAllParticles()
        {
            Texture2D texture = TextureAssets.Extra[98].Value;
			Rectangle sourceRectangle = texture.Frame(1, 1);
			Vector2 origin = sourceRectangle.Size() / 2f;			
			
            for (int i = 0; i < particles.Count; i++)
            {
				Color color = new Color(120, 184, 255, 50) * (particles[i].ProgressOneToZero * 1.5f);
				float scale = particles[i].ProgressOneToZero*particles[i].scale;
				
                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].ProgressOneToZero*4f+MathHelper.PiOver4, scale);
				particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].ProgressOneToZero*4f-MathHelper.PiOver4, scale);
            }
        }
    }
}
