using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ITD.Content.Tiles;
using ITD.Content.World.WorldGenUtils;
using Terraria.Localization;
using System;
using ITD.Content.Walls;

namespace ITD.Content.World
{
    public class DeepDesertGenPass : GenPass
    {
        private static int diorite;
        private static int pegmatite;
        private static int pegmatiteWall;
        public DeepDesertGenPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            diorite = ModContent.TileType<DioriteTile>();
            pegmatite = ModContent.TileType<PegmatiteTile>();
            pegmatiteWall = ModContent.WallType<PegmatiteWallUnsafe>();
            Point p = GetGenStartPoint();
            GenDeepDesert(p.X, p.Y, WorldGen._genRandSeed);
        }

        private Point GetGenStartPoint()
        {
            Point deepDesertStart = new();
            for (int j = Main.maxTilesY; j >= 0 && deepDesertStart == Point.Zero; j--)
            {
                for (int i = Main.maxTilesX; i >= 0; i--)
                {
                    if (Main.tile[i, j].TileType == TileID.Sandstone)
                    {
                        deepDesertStart = new Point(i, j);
                        break;
                    }
                }
            }
            return deepDesertStart;
        }

        private void GenDeepDesert(float x, float y, int seed)
        {
            int width = Main.maxTilesX / 15;
            int height = Main.maxTilesY / 6;
            var rectangle = new Rectangle((int)x - (width / 2), (int)y - (height / 2), width, height);
            static bool inCircle(int i, int j, Rectangle rect, int wid)
            {
                Point rC = rect.Center;
                int xElem = i - rC.X;
                int yElem = j - rC.Y;
                int r = wid / 2;
                return xElem * xElem + yElem * yElem <= r * r;
            }
            List<Rectangle> tunnelsList = new List<Rectangle>();
            bool inTunnels(int i, int j)
            {
                for(int k = 0; k < tunnelsList.Count; k++)
                {
                    if (tunnelsList[k].Contains(i, j))
                    {
                        return true;
                    }
                }
                return false;
            }
            WorldUtils.Gen(rectangle.Center, new Shapes.Circle(width/2, height/2), Actions.Chain(
            [
                new Actions.SetTile((ushort)diorite),
                new Actions.PlaceWall((ushort)pegmatiteWall)
            ]));
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    if (inCircle(i, j, rectangle, width) && Helpers.TileType(i, j, diorite))
                    {
                        if (WorldGen.genRand.NextBool(500) && !inTunnels(i,j))
                        {
                            int dir = Math.Sign((rectangle.Center.ToVector2() - new Vector2(i, j)).X);
                            //WorldGen.digTunnel(i, j, dir, 0, 150, 4);
                            int padding = 54;
                            tunnelsList.Add(new Rectangle(i-padding, j-padding, 200+padding*2, 6+padding*2));
                            int steps = WorldGen.genRand.Next(90, 300);
                            for (int k = 0; k < steps; k++)
                            {
                                WorldGen.digTunnel(i + (dir==1?k:-k), j, dir, 0, 5, 6);
                            }
                            WorldGen.digTunnel(i + WorldGen.genRand.Next(0, steps), j, 0, 2, 60, 8);
                            //WorldGen.ReplaceTile(i, j, (ushort)ModContent.TileType<DioriteTile>(), 0);
                        }
                    }
                }
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    bool isTileEdgeTile = Framing.GetTileSafely(i, j).HasTile && (!Framing.GetTileSafely(i+1, j).HasTile|| !Framing.GetTileSafely(i - 1, j).HasTile || !Framing.GetTileSafely(i, j + 1).HasTile || !Framing.GetTileSafely(i, j - 1).HasTile);
                    if (isTileEdgeTile && Helpers.TileType(i, j, diorite))
                    {
                        WorldGen.TileRunner(i, j, 5, 2, pegmatite);
                    }
                }
            }
        }
    }
}
