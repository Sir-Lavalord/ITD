using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using Terraria.ID;
using Terraria.GameContent.Drawing;

namespace ITD.Systems
{
    // Adapted from the Verdant Mod
    public static class WeatherSystem
    {
        internal static void DrawGrassSway(SpriteBatch batch, Texture2D texture, int i, int j, Color? color = null, Vector2 drawOffset = default)
        {
            Tile tile = Main.tile[i, j];
            Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + TileHelpers.CommonTileOffset;

            float rot = GetGrassSway(i, j, ref pos);
            Vector2 orig = GrassOrigin(i, j);
            Color color0 = Lighting.GetColor(i, j);
            if (color != null) color0 = (Color)color;

            batch.Draw(texture, pos + new Vector2(8, 16), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 22), color0, rot, orig, 1f, SpriteEffects.None, 0f);
        }
        internal static Vector2 GrassOrigin(int i, int j)
        {
            short _ = 0;
            Tile tile = Main.tile[i, j];
            Main.instance.TilesRenderer.GetTileDrawData(i, j, tile, tile.TileType, ref _, ref _, out var tileWidth, out var _, out var tileTop, out var halfBrickHeight, out var _, out var _, out var _, out var _, out var _, out var _);
            return new Vector2(tileWidth / 2, 16 - halfBrickHeight - tileTop);
        }
        internal static float GetGrassSway(int i, int j, ref Vector2 position)
        {
            Tile tile = Main.tile[i, j];
            TileDrawing tilesRenderer = Main.instance.TilesRenderer;
            float rotation = tilesRenderer.GetWindCycle(i, j, 0.1);

            if (!WallID.Sets.AllowsWind[tile.WallType])
                rotation = 0f;
            if (!WorldGen.InAPlaceWithWind(i, j, 1, 1))
                rotation = 0f;
            rotation += Main.instance.TilesRenderer.GetWindGridPush(i, j, 20, 0.35f);

            position.X += rotation;
            position.Y += Math.Abs(rotation);
            return rotation * 0.1f;
        }
        internal static void DrawTreeSway(int i, int j, SpriteBatch spriteBatch, Texture2D tex, Rectangle? source, Vector2? offset = null, Vector2? origin = null, SpriteEffects effect = SpriteEffects.None)
        {
            Tile tile = Main.tile[i, j];
            Vector2 drawPos = TileHelpers.TileExtraPos(i, j) + (offset ?? Vector2.Zero);

            float rot = GetTreeSway(i, j, ref drawPos);
            Color col = Lighting.GetColor(i, j);

            if (tile.TileColor == PaintID.IlluminantPaint)
                col = Color.White;

            spriteBatch.Draw(tex, drawPos, source, col, rot * 0.08f, origin ?? source.Value.Size() / 2f, 1f, effect, 0f);
        }
        private static float GetTreeSway(int i, int j, ref Vector2 pos)
        {
            TileDrawing tilesRenderer = Main.instance.TilesRenderer;
            float rot = tilesRenderer.GetWindCycle(i, j, 0.1);

            pos.X += rot * 2f;
            pos.Y += Math.Abs(rot) * 2f;
            return rot;
        }
    }
}
