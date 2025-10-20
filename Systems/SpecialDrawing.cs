using ReLogic.Graphics;

namespace ITD.Systems;

public static class SpecialDrawing
{
    public static void RestartSpriteBatchAsTileDrawer(RenderTarget2D inbetweenerTarget = null, RenderTargetBinding[] previousTargets = null)
    {
        Main.spriteBatch.End();
        if (inbetweenerTarget != null)
        {
            Main.graphics.GraphicsDevice.SetRenderTarget(inbetweenerTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);
        }
        if (previousTargets != null)
            Main.graphics.GraphicsDevice.SetRenderTargets(previousTargets);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);
    }
    public static void RestartSpriteBatchAsRegular()
    {
        // help, why doesn't Main.GameViewMatrix.TransformMatrix work as its supposed to. it only works with EffectMatrix
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, default, Main.GameViewMatrix.EffectMatrix);
    }
    public static void DrawCenteredString(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 position, float scale)
    {
        spriteBatch.DrawString(font, text, position, Main.MouseTextColorReal, 0f, font.MeasureString(text) * 0.5f, scale, SpriteEffects.None, 0f);
    }
}
