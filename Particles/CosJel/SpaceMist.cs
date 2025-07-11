using System.Runtime.InteropServices;

namespace ITD.Particles.CosJel
{
    public class SpaceMist : ParticleEmitter
    {
        internal static Asset<Texture2D> outlineTex;
        /*
        public override void SetDefaults()
        {
            canvas = ParticleEmitterDrawCanvas.WorldUnderProjectiles;
            scale *= 1.3f;
            timeLeft = 60;
        }
        */
        public override void SetStaticDefaults()
        {
            outlineTex = Mod.Assets.Request<Texture2D>("Particles/Textures/SpaceMist_Outline");
        }
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
            Texture2D tex = outlineTex.Value;
            Color color1 = new(255, 242, 191);
            Color color2 = new(168, 241, 255);

            foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
            {
                particle.DrawCommon(in Main.spriteBatch, in tex, CanvasOffset, Color.Lerp(color1, color2, Utils.PingPongFrom01To010(MathHelper.Lerp(0, 1, timeLeft / 60f))));
            }
        }
    }
}
