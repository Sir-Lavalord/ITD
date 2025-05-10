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
using Terraria.Graphics.Renderers;

namespace ITD.Particles.Projectile
{
    public class BleedbreakerHitFlash : ParticleEmitter
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
            particle.scale = 4f;
        }
        public override void AI(ref ITDParticle particle)
        {
            particle.opacity = particle.ProgressOneToZero;
        }
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void DrawAllParticles()
        {
            Texture2D texture = TextureAssets.Extra[98].Value;
            Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < particles.Count; i++)
            {
                Color color = Color.Red * (particles[i].ProgressOneToZero * 1.5f);
                float scale = particles[i].ProgressOneToZero * particles[i].scale;
                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].ProgressOneToZero * 4f, scale);
                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].ProgressOneToZero * 5f + MathHelper.Pi/3, scale);
                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].ProgressOneToZero * 5f - MathHelper.Pi / 3, scale);
            }
        }
    }
}
