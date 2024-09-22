using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using System.Collections.Generic;

namespace ITD.Systems
{
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
    }
}
