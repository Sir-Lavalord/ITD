using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
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
        public static void DrawAdjustableBox(SpriteBatch spriteBatch, Texture2D tex, Rectangle rect, Color col)
        {
            Vector2 quadSize = new(tex.Width / 3, tex.Height / 3);
            var xScale = (rect.Width - quadSize.X * 2) / quadSize.X;
            var yScale = (rect.Height - quadSize.Y * 2) / quadSize.Y;
            // Draw center
            Rectangle centerFrame = tex.Frame(3, 3, 1, 1);
            spriteBatch.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y + quadSize.Y), centerFrame, col, 0, default, new Vector2(xScale, yScale), SpriteEffects.None, 0f);
            // Draw sides
            Rectangle topSideFrame = tex.Frame(3, 3, 1);
            spriteBatch.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y), topSideFrame, col, 0, default, new Vector2(xScale, 1), SpriteEffects.None, 0f);
            Rectangle leftSideFrame = tex.Frame(3, 3, 0, 1);
            spriteBatch.Draw(tex, new Vector2(rect.X, rect.Y + quadSize.Y), leftSideFrame, col, 0, default, new Vector2(1, yScale), SpriteEffects.None, 0f);
            Rectangle rightSideFrame = tex.Frame(3, 3, 2, 1);
            spriteBatch.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y + quadSize.Y), rightSideFrame, col, 0, default, new Vector2(1, yScale), SpriteEffects.None, 0f);
            Rectangle bottomSideFrame = tex.Frame(3, 3, 1, 2);
            spriteBatch.Draw(tex, new Vector2(rect.X + quadSize.X, rect.Y + rect.Height - quadSize.Y), bottomSideFrame, col, 0, default, new Vector2(xScale, 1), SpriteEffects.None, 0f);
            // Draw corners
            Rectangle topLeftCorner = tex.Frame(3, 3);
            spriteBatch.Draw(tex, new Vector2(rect.X, rect.Y), topLeftCorner, col, 0, default, 1, SpriteEffects.None, 0f);
            Rectangle topRightCorner = tex.Frame(3, 3, 2);
            spriteBatch.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y), topRightCorner, col, 0, default, 1, SpriteEffects.None, 0f);
            Rectangle bottomLeftCorner = tex.Frame(3, 3, 0, 2);
            spriteBatch.Draw(tex, new Vector2(rect.X, rect.Y + rect.Height - quadSize.Y), bottomLeftCorner, col, 0, default, 1, SpriteEffects.None, 0f);
            Rectangle bottomRightCorner = tex.Frame(3, 3, 2, 2);
            spriteBatch.Draw(tex, new Vector2(rect.X + rect.Width - quadSize.X, rect.Y + rect.Height - quadSize.Y), bottomRightCorner, col, 0, default, 1, SpriteEffects.None, 0f);
        }
    }
}
