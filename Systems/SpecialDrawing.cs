using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ITD.Systems
{
    public static class SpecialDrawing
    {
        public static void RestartSpriteBatchAsTileDrawer()
        {
            Main.spriteBatch.End();
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
