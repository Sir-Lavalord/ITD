using Terraria.GameContent;
using System.Runtime.InteropServices;

namespace ITD.Particles.Projectile
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
            // Rectangle sourceRectangle = texture.Frame(1, 1);
            Vector2 origin = texture.Size() * 0.5f;

            foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
            {
                Color color = new Color(255, 255, 255, 50) * (particle.ProgressOneToZero * 1.5f);
                float scale = particle.ProgressOneToZero * particle.scale;

                particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, null, origin, particle.rotation, scale);
            }
        }
    }
}
