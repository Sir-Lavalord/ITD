using ITD.Utilities.EntityAnim;
using System;
using System.Runtime.InteropServices;

namespace ITD.Particles.Ambience
{
    public class LyteflyParticle : ParticleEmitter
    {
        internal static Asset<Texture2D> outlineTex;
        public const float ParticleSpeed = 2f;
        public override void SetStaticDefaults()
        {
            outlineTex = Mod.Assets.Request<Texture2D>("Particles/Textures/LyteflyParticle_Glow");

            ParticleSystem.particleFramesVertical[type] = 2;
        }
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.opacity = 0f;
            particle.velocity = Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)) * ParticleSpeed;
        }
        public override void AI(ref ITDParticle particle)
        {
            float progress = particle.ProgressZeroToOne;
            float fadeDuration = 0.1f;
            float factor;

            if (progress <= fadeDuration)
            {
                factor = EasingFunctions.OutQuad(progress / fadeDuration);
            }
            else if (progress >= 1f - fadeDuration)
            {
                factor = EasingFunctions.OutQuad((1f - progress) / fadeDuration);
            }
            else
            {
                factor = 1f;
            }
            particle.opacity = factor;
            particle.velocity = Vector2.Lerp(particle.velocity, particle.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(1f) * ParticleSpeed, 0.5f);
            particle.spriteEffects = particle.velocity.X > 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            if (++particle.frameCounter > 4)
                particle.frameVertical = (byte)(++particle.frameVertical % ParticleSystem.particleFramesVertical[type]);
        }
        public override void PreDrawAllParticles()
        {
            Texture2D tex = outlineTex.Value;
            foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
            {
                float scale = 0.4f + MathF.Sin((float)Main.timeForVisualEffects / 32f + (particle.timeLeft / 16f)) * 0.2f;
                Color color = (Color.Yellow with { A = 0 }) * particle.opacity;
                particle.DrawCommon(in Main.spriteBatch, in tex, CanvasOffset, color, tex.Bounds, tex.Size() * 0.5f, scale: scale);
            }
        }
    }
}
