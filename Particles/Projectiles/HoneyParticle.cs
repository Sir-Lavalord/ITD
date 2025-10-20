using System.Runtime.InteropServices;

namespace ITD.Particles.Projectiles;

public class HoneyParticle : ParticleEmitter
{
    internal static Asset<Texture2D> outlineTex;
    public override void SetStaticDefaults()
    {
        outlineTex = Mod.Assets.Request<Texture2D>("Particles/Textures/HoneyParticle_Outline");

        ParticleSystem.particleUsesRenderTarget[type] = true;
    }
    public override void OnEmitParticle(ref ITDParticle particle)
    {
        particle.scale *= 1.6f;
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
        foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
        {
            particle.DrawCommon(in Main.spriteBatch, in tex, CanvasOffset);
        }
    }
}
