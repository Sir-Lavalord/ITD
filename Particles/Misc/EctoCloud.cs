using System.Runtime.InteropServices;

namespace ITD.Particles.Misc;

public class EctoCloud : ParticleEmitter
{
    /*
    public override void SetDefaults()
    {
        canvas = ParticleEmitterDrawCanvas.WorldUnderProjectiles;
        scale *= 1.3f;
        timeLeft = 60;
    }
    */
    public override void OnEmitParticle(ref ITDParticle particle)
    {
        particle.scale *= 2f;
        // timeleft is now set on Emit() call. canvas is set on Emitter initialization.
    }
    public override void AI(ref ITDParticle particle)
    {
        particle.scale = particle.spawnParameters.Scale * particle.ProgressOneToZero;
        particle.velocity *= 0.95f;
    }
    public override Color GetAlpha(ITDParticle particle) => Color.White;
    public override void DrawAllParticles()
    {
        Texture2D tex = Texture;
        Color color1 = new(9, 121, 255);
        Color color2 = new(0, 220, 255);
        Color color3 = new(255, 255, 255);

        // storing the span is oddly less performant though i'm sure there's an actual explanation for it
        foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
        {
            particle.DrawCommon(in Main.spriteBatch, in tex, CanvasOffset, color1);
        }
        foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
        {
            particle.DrawCommon(in Main.spriteBatch, in tex, CanvasOffset, color2, scale: particle.scale * 0.8f);
        }
        foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
        {
            particle.DrawCommon(in Main.spriteBatch, in tex, CanvasOffset, color3, scale: particle.scale * 0.6f);
        }
    }
}
