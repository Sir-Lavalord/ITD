using Terraria.GameContent;
using System.Runtime.InteropServices;

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
            // Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = texture.Size() * 0.5f;

            foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
            {
                Color color = col * (particle.ProgressOneToZero * 1.5f);
                float scale = (particle.ProgressOneToZero * particle.scale) / 1.1f;
                particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, null, origin, particle.rotation + MathHelper.PiOver2, scale * 0.5f);

                particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, null, origin, particle.rotation, scale * 0.5f);
            }
        }
    }
}
