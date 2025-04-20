using ITD.Utilities;
using Terraria;
using Terraria.ID;

namespace ITD.Particles.Projectile
{
    public class WispFlame : ParticleEmitter
    {
        public override void SetStaticDefaults()
        {
            //ParticleSystem.particleFramesVertical[type] = 6;
            base.SetStaticDefaults();
        }
        public override Color GetAlpha(ITDParticle particle)
        {
            return Color.White;
        }
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale *= Main.rand.NextFloat(1f, 1.2f);
        }
        public override void AI(ref ITDParticle particle)
        {
            //if (++particle.frameCounter >= 3)
            //{
            //    particle.frameCounter = 0;
            //    if (++particle.frameVertical >= 6)
            //    {
            //        particle.frameVertical = 0;
            //    }
            //}
			particle.scale -= 0.05f;
        }
		public override void DrawAllParticles()
        {
            Rectangle sourceRectangle = Texture.Frame(1, 1);
            Vector2 origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < particles.Count; i++)
            {
				for (int j = 0; j < 5; j++) {
					Color color = new Color(75, 75, 75, 0);
					particles[i].DrawCommon(Main.spriteBatch, Texture, CanvasOffset + (Main.rand.NextVector2Square(-6f, 6f)*particles[i].scale), color, sourceRectangle, origin, particles[i].rotation, particles[i].scale);
				}
			}
        }
    }
}
