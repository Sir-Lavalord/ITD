using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using ITD.Utilities;
using Terraria.ID;

namespace ITD.Systems
{
    public class WeatherSystem : ModSystem
    {
        // adapted from vanilla, and, once again, the verdant mod (REMEMBWER TO USE GPLV4 LICENSE ON RELEASE AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA)
        // TODO: make it not choppy. somehow.
        public double TreeWind { get; private set; }
        public double GrassWind { get; private set; }
        public override void PreUpdateWorld()
        {
            if (!Main.dedServ)
            {
                double num = Math.Abs(Main.WindForVisuals);
                num = Utils.GetLerpValue(0.08f, 1.2f, (float)num, clamped: true);
                TreeWind += 1.0 / 240.0 + 1.0 / 240.0 * num * 2.0;
                GrassWind += 1.0 / 180.0 + 1.0 / 180.0 * num * 4.0;
            }
        }
        internal static void DrawGrassSway(SpriteBatch batch, Texture2D texture, int i, int j, Color color)
        {
            Tile tile = Main.tile[i, j];
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;

            float rot = ModContent.GetInstance<WeatherSystem>().GetGrassSway(i, j, ref pos);
            //Main.NewText(rot);
            Vector2 orig = GrassOrigin(i, j);

            batch.Draw(texture, pos + new Vector2(8, 16), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 22), color, rot, orig, 1f, SpriteEffects.None, 0f);
        }
        internal static Vector2 GrassOrigin(int i, int j)
        {
            short _ = 0;
            Tile tile = Main.tile[i, j];
            Main.instance.TilesRenderer.GetTileDrawData(i, j, tile, tile.TileType, ref _, ref _, out var tileWidth, out var _, out var tileTop, out var halfBrickHeight, out var _, out var _, out var _, out var _, out var _, out var _);
            return new Vector2(tileWidth / 2, 16 - halfBrickHeight - tileTop);
        }
        internal float GetGrassSway(int i, int j, ref Vector2 position)
        {
            Tile tile = Main.tile[i, j];

            float rotation = Main.instance.TilesRenderer.GetWindCycle(i, j, GrassWind);
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
            Vector2 drawPos = Helpers.TileExtraPos(i, j) + (offset ?? Vector2.Zero);
            float rot = ModContent.GetInstance<WeatherSystem>().GetTreeSway(i, j, ref drawPos);
            Color col = Lighting.GetColor(i, j);

            if (tile.TileColor == 31)
                col = Color.White;
            // GOD WHO TF CODED THE MODTILE DRAWING THIS IS FUCKED UP
            // WHY DOESN'T THE SPRITEBATCH HAVE THE SAME PARAMETERS AS REGULAR TILE DRAWING
            // GOD
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            spriteBatch.Draw(tex, drawPos, source, col, rot * 0.08f, origin ?? source.Value.Size() / 2f, 1f, effect, 0f);
        }
        private float GetTreeSway(int i, int j, ref Vector2 pos)
        {
            float rot = Main.instance.TilesRenderer.GetWindCycle(i, j, TreeWind);
            pos.X += rot * 2f;
            pos.Y += Math.Abs(rot) * 2f;
            return rot;
        }
    }
}
