using System.Runtime.InteropServices;
using Terraria.GameContent;

namespace ITD.Particles.Projectiles;

public class BleedbreakerHitFlash : ParticleEmitter
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
        particle.scale = 4f;
    }
    public override void AI(ref ITDParticle particle)
    {
        particle.opacity = particle.ProgressOneToZero;
    }
    public override Color GetAlpha(ITDParticle particle) => Color.White;
    public override void DrawAllParticles()
    {
        Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
        // Rectangle sourceRectangle = texture.Frame(1, 1); // idk if this had an actual intention behind it or not so i'll keep it commented out
        Vector2 origin = texture.Size() * 0.5f;

        foreach (ITDParticle particle in CollectionsMarshal.AsSpan(particles))
        {
            Color color = Color.Red * (particle.ProgressOneToZero * 1.5f);
            float scale = particle.ProgressOneToZero * particle.scale;
            particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, null, origin, particle.ProgressOneToZero * 4f, scale);
            particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, null, origin, particle.ProgressOneToZero * 5f + MathHelper.Pi / 3, scale);
            particle.DrawCommon(in Main.spriteBatch, in texture, CanvasOffset, color, null, origin, particle.ProgressOneToZero * 5f - MathHelper.Pi / 3, scale);
        }
    }
}
