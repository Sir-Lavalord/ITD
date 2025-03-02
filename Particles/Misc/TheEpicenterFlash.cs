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
    //swiped off tapenki's code
    public class TheEpicenterFlash : ParticleEmitter
    {
        public override void SetStaticDefaults()
        {
            ParticleSystem.particleUsesRenderTarget[type] = true;
        }
        public override void OnEmitParticle(ref ITDParticle particle)
        {
        }
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void DrawAllParticles()
        {
            Texture2D texture = TextureAssets.Extra[98].Value;
            Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < particles.Count; i++)
            {
                Color color = new Color(255, 255, 255, 50) * (particles[i].ProgressOneToZero * 1.5f);
                float scale = particles[i].ProgressOneToZero * particles[i].scale;

                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].rotation, scale);
            }
        }
    }
}
