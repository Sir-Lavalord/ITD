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

namespace ITD.Particles.Projectile
{
    //swiped off tapenki's code x2
    public class TwilightDemiseFlash : ParticleEmitter
    {

        public override void SetStaticDefaults()
        {
            ParticleSystem.particleUsesRenderTarget[type] = true;
        }
        Color col = new Color(255, 255, 255, 50);
        public override Color GetAlpha(ITDParticle particle) => Color.White;
        public override void DrawAllParticles()
        {
            Texture2D texture = TextureAssets.Extra[98].Value;
            Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < particles.Count; i++)
            {
                Color color = col * (particles[i].ProgressOneToZero * 1.5f);
                float scale = (particles[i].ProgressOneToZero * particles[i].scale) / 1.1f;
                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].rotation + MathHelper.PiOver2, scale/2);

                particles[i].DrawCommon(Main.spriteBatch, texture, CanvasOffset, color, sourceRectangle, origin, particles[i].rotation, scale/2);
            }
        }
    }
}
