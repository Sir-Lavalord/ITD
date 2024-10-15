using Microsoft.Xna.Framework.Graphics;

namespace ITD.Kinematics
{
    public abstract class KineChain(float x, float y)
    {
        public abstract KineSegment[] Segments { get; }
        public Vector2 ChainBase { get; set; } = new(x, y);
        public void Update(Vector2 targetPos)
        {
            Segments[0].Update();
            Segments[0].Follow(targetPos);

            for (int i = 1; i < Segments.Length; i++)
            {
                Segments[i].Update();
                Segments[i].FollowSegment(Segments[i - 1]);
            }
            int last = Segments.Length - 1;
            KineSegment s = Segments[last];
            s.a = ChainBase;
            s.Update();

            for (int i = last - 1; i >= 0; i--)
            {
                KineSegment seg = Segments[i];
                KineSegment next = Segments[i + 1];
                seg.a = next.b;
                seg.Update();
            }
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color, bool shouldBeMirrored, Texture2D texture, Texture2D startTexture = null, Texture2D endTexture = null)
        {
            for (int i = Segments.Length - 1; i >= 0; i--)
            {
                Texture2D textureToDraw = i == Segments.Length - 1 && startTexture != null ? startTexture : i == 0 && endTexture != null ? endTexture : texture;
                Segments[i].Draw(spriteBatch, screenPos, color, textureToDraw, shouldBeMirrored);
            }
        }
    }
}
