using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace ITD.Kinematics
{
    public class KineSegment(float x, float y, float length)
    {
        public Vector2 a { get; set; } = new Vector2(x, y);
        public Vector2 b { get; set; }
        public float Length { get; set; } = length;
        public float Angle { get; set; }
        public float MinAngle { get; set; } = 0f;
        public float MaxAngle { get; set; } = MathF.Tau;
        public float LocalAngle;
        public Vector2 Target { get; set; } = Main.MouseWorld;

        public void FollowSegment(KineSegment child)
        {
            Follow(new Vector2(child.a.X, child.a.Y));
        }
        public void Follow(Vector2 target)
        {
            Vector2 dir = target - a;
            Angle = dir.ToRotation();
            dir.Normalize();
            dir *= Length;
            dir *= -1f;
            a = target + dir;
        }

        public void Update()
        {
            float dx = Length * (float)Math.Cos(Angle);
            float dy = Length * (float)Math.Sin(Angle);
            b = new Vector2(a.X + dx, a.Y + dy);
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color, Texture2D texture, bool shouldBeMirrored)
        {
            Point tileCoords = Vector2.Lerp(a, b, 0.5f).ToTileCoordinates();
            spriteBatch.Draw(texture, a - screenPos, null, Lighting.GetColor(tileCoords.X, tileCoords.Y).MultiplyRGB(color), Angle, new Vector2(0f, texture.Height / 2f), 1f, shouldBeMirrored ? SpriteEffects.FlipVertically : SpriteEffects.None, default);
        }
    }
}
