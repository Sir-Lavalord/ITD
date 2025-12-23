using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using Terraria.GameContent;

namespace ITD.Particles.CosJel;

public class DashSpark : ParticleEmitter
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
        Texture2D texture = TextureAssets.Extra[ExtrasID.SharpTears].Value;
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
