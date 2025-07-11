using Terraria.GameContent;
using System.Runtime.InteropServices;

namespace ITD.Particles.Projectile
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
			
            foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
            {
                Color color = new Color(120, 184, 255, 50) * (particle.ProgressOneToZero * 1.5f);
                float scale = particle.ProgressOneToZero * particle.scale;

                particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, sourceRectangle, origin, particle.ProgressOneToZero * 4f + MathHelper.PiOver4, scale);
                particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, sourceRectangle, origin, particle.ProgressOneToZero * 4f - MathHelper.PiOver4, scale);
            }
        }
    }
}
