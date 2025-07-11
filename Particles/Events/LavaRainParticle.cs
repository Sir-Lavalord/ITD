using ITD.Utilities;

namespace ITD.Particles.Events
{
    public class LavaRainParticle : ParticleEmitter
    {
        public override void SetStaticDefaults()
        {
            ParticleSystem.particleFramesHorizontal[type] = 3;
            base.SetStaticDefaults();
        }
        public override Color GetAlpha(ITDParticle particle)
        {
            return Color.Lerp(Lighting.GetColor(particle.position.ToTileCoordinates()), Color.White, 0.5f);
        }
        public override void OnEmitParticle(ref ITDParticle particle)
        {
            particle.scale *= Main.rand.NextFloat(0.5f, 0.8f);
            particle.frameHorizontal = (byte)Main.rand.Next(3);
        }
        public override void AI(ref ITDParticle particle)
        {
            Vector2 translatedPosition = particle.position + new Vector2(0f, 42f) * particle.scale;
            if (particle.timeLeft < 30)
            {
                if (particle.timeLeft > 2 && TileHelpers.SolidTile(translatedPosition.ToTileCoordinates()))
                {
                    particle.timeLeft = 2;
                }
            }
            if (particle.timeLeft == 1)
            {
                int amountOfDusts = 3;
                for (int i = 0; i < amountOfDusts; i++)
                {
                    float angleRange = MathHelper.PiOver2;
                    float rotRadians = angleRange / (float)amountOfDusts;
                    Vector2 velocity = (angleRange - rotRadians).ToRotationVector2().RotatedBy(rotRadians * i) * 4f;
                    Dust.NewDust(particle.position, 1, 1, DustID.Torch, velocity.X, velocity.Y);
                }
            }
        }
    }
}
