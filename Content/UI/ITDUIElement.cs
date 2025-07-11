using System;
using Terraria.GameContent;
using Terraria.UI;

namespace ITD.Content.UI
{
    // i did something similar for olympus just to make things easier
    // i looove wrapper classes
    public abstract class ITDUIElement : UIElement, ILocalizedModType
    {
        string ILocalizedModType.LocalizationCategory => "UI";
        Mod IModType.Mod => ITD.Instance;
        string IModType.Name => GetType().Name;
        string IModType.FullName => $"{ITD.Instance.Name}/{GetType().Name}";
        public void SetProperties(float top, float left, float width, float height)
        {
            Top.Set(top, 0f);
            Left.Set(left, 0f);
            Width.Set(width, 0f);
            Height.Set(height, 0f);
        }
        public void DrawDebugRectangle(SpriteBatch sb, Color color)
        {
            sb.Draw(TextureAssets.MagicPixel.Value, GetDimensions().ToRectangle(), color);
        }
        public static void DrawAdjustableBox(SpriteBatch spriteBatch, Texture2D tex, Rectangle rect, Color col, SkipDrawBoxSegment? flags = null)
        {
            Vector2 quadSize = new(tex.Width / 3, tex.Height / 3);
            // scales for the extendable bits of the box.
            // as an important note, you should probably try to avoid the corners and sides squashing for real applications,
            // but as a failsafe, i've added these to make sure an adjustablebox never looks weird.
            float cornerScaleX = Math.Min(1, rect.Width / (quadSize.X * 2));
            float cornerScaleY = Math.Min(1, rect.Height / (quadSize.Y * 2));
            float sideScaleX = Math.Max(0, (rect.Width - quadSize.X * 2) / quadSize.X);
            float sideScaleY = Math.Max(0, (rect.Height - quadSize.Y * 2) / quadSize.Y);

            void DrawSegment(Vector2 position, Rectangle frame, Vector2 scale)
            {
                spriteBatch.Draw(tex, position, frame, col, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            // Draw center
            if (flags is null || !((flags.Value & SkipDrawBoxSegment.CenterCenter) == SkipDrawBoxSegment.CenterCenter))
            {
                Rectangle centerFrame = tex.Frame(3, 3, 1, 1);
                DrawSegment(new Vector2(rect.X + quadSize.X * cornerScaleX, rect.Y + quadSize.Y * cornerScaleY), centerFrame, new Vector2(sideScaleX, sideScaleY));
            }

            // Draw sides
            if (flags is null || !((flags.Value & SkipDrawBoxSegment.TopCenter) == SkipDrawBoxSegment.TopCenter))
            {
                Rectangle topSideFrame = tex.Frame(3, 3, 1, 0);
                DrawSegment(new Vector2(rect.X + quadSize.X * cornerScaleX, rect.Y), topSideFrame, new Vector2(sideScaleX, cornerScaleY));
            }

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.CenterLeft) == SkipDrawBoxSegment.CenterLeft))
            {
                Rectangle leftSideFrame = tex.Frame(3, 3, 0, 1);
                DrawSegment(new Vector2(rect.X, rect.Y + quadSize.Y * cornerScaleY), leftSideFrame, new Vector2(cornerScaleX, sideScaleY));
            }

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.CenterRight) == SkipDrawBoxSegment.CenterRight))
            {
                Rectangle rightSideFrame = tex.Frame(3, 3, 2, 1);
                DrawSegment(new Vector2(rect.X + rect.Width - quadSize.X * cornerScaleX, rect.Y + quadSize.Y * cornerScaleY), rightSideFrame, new Vector2(cornerScaleX, sideScaleY));
            }

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.BottomCenter) == SkipDrawBoxSegment.BottomCenter))
            {
                Rectangle bottomSideFrame = tex.Frame(3, 3, 1, 2);
                DrawSegment(new Vector2(rect.X + quadSize.X * cornerScaleX, rect.Y + rect.Height - quadSize.Y * cornerScaleY), bottomSideFrame, new Vector2(sideScaleX, cornerScaleY));
            }

            // Draw corners
            Vector2 cornerScale = new(cornerScaleX, cornerScaleY);

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.TopLeft) == SkipDrawBoxSegment.TopLeft))
            {
                Rectangle topLeftCorner = tex.Frame(3, 3, 0, 0);
                DrawSegment(new Vector2(rect.X, rect.Y), topLeftCorner, cornerScale);
            }

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.TopRight) == SkipDrawBoxSegment.TopRight))
            {
                Rectangle topRightCorner = tex.Frame(3, 3, 2, 0);
                DrawSegment(new Vector2(rect.X + rect.Width - quadSize.X * cornerScaleX, rect.Y), topRightCorner, cornerScale);
            }

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.BottomLeft) == SkipDrawBoxSegment.BottomLeft))
            {
                Rectangle bottomLeftCorner = tex.Frame(3, 3, 0, 2);
                DrawSegment(new Vector2(rect.X, rect.Y + rect.Height - quadSize.Y * cornerScaleY), bottomLeftCorner, cornerScale);
            }

            if (flags is null || !((flags.Value & SkipDrawBoxSegment.BottomRight) == SkipDrawBoxSegment.BottomRight))
            {
                Rectangle bottomRightCorner = tex.Frame(3, 3, 2, 2);
                DrawSegment(new Vector2(rect.X + rect.Width - quadSize.X * cornerScaleX, rect.Y + rect.Height - quadSize.Y * cornerScaleY), bottomRightCorner, cornerScale);
            }
        }
    }
    [Flags]
    public enum SkipDrawBoxSegment
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 4,
        CenterLeft = 8,
        CenterCenter = 16,
        CenterRight = 32,
        BottomLeft = 64,
        BottomCenter = 128,
        BottomRight = 256,
    }
}
