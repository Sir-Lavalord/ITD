using Microsoft.Xna.Framework.Graphics;

namespace ITD.Particles
{
    public readonly record struct ParticleSpawnParameters(short TimeLeft, float Scale);
    public struct ITDParticle()
    {
        public Vector2 position;
        public Vector2 velocity;
        public float rotation;
        public float angularVelocity;
        public float scale = 1f;
        public float opacity = 1f;
        public SpriteEffects spriteEffects = SpriteEffects.None;
        public byte frameVertical;
        public byte frameHorizontal;
        public byte frameCounter;
        public short timeLeft;
        public ParticleEmitter parent;
        public ParticleSpawnParameters spawnParameters;
        public readonly float ProgressZeroToOne => (spawnParameters.TimeLeft - timeLeft) / (float)spawnParameters.TimeLeft;
        public readonly float ProgressOneToZero => 1f - ProgressZeroToOne;
        public readonly void DrawCommon(in SpriteBatch sb, in Texture2D texture, Vector2 offset = default, Color? color = null, Rectangle? sourceRect = null, Vector2? origin = null, float? rotation = null, float? scale = null, SpriteEffects? spriteEffects = null)
        {
            ParticleFramingData framingData = parent.GetFramingData(this);

            Color drawColor = color ?? parent.GetAlpha(this);
            if (parent.additive)
                drawColor.A = 0;

            sb.Draw(texture, position - offset, sourceRect ?? framingData.SourceRect, drawColor * opacity, rotation ?? this.rotation, origin ?? framingData.Origin, scale ?? this.scale, spriteEffects ?? this.spriteEffects, 0f);
        }
    }
    public readonly record struct ParticleFramingData(Rectangle SourceRect, Vector2 Origin);
}
