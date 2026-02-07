using System.Runtime.InteropServices;

namespace ITD.Particles.Misc;

public class WispMist : ParticleEmitter
{
    internal static Asset<Texture2D> outlineTex;
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
        outlineTex = Mod.Assets.Request<Texture2D>("Particles/Textures/WispMist_Outline");

        ParticleSystem.particleUsesRenderTarget[type] = true;
    }
    public override void OnEmitParticle(ref ITDParticle particle)
    {
        particle.scale *= 2f;
        
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
