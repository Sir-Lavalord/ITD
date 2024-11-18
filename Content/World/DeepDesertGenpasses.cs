using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;
using System;
using ITD.Content.Walls;
using ITD.Content.Tiles.DeepDesert;
using ITD.Utilities;
using static Terraria.WorldGen;
using static ITD.Utilities.WorldGenHelpers.Procedural;
using System.Linq;

namespace ITD.Content.World
{
    public class DeepDesertGenPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        private static ushort darkPyracotta;
        private static ushort pegmatite;
        private static ushort pegmatiteWall;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            darkPyracotta = (ushort)ModContent.TileType<DioriteTile>();
            pegmatite = (ushort)ModContent.TileType<PegmatiteTile>();
            pegmatiteWall = (ushort)ModContent.WallType<PegmatiteWallUnsafe>();
            Point p = GetGenStartPoint();
            GenDeepDesert(p.X, p.Y, WorldGen._genRandSeed);
        }

        private static Point GetGenStartPoint()
        {
            return GenVars.UndergroundDesertLocation.Bottom().ToPoint();
        }

        private static void GenDeepDesert(int x, int y, int seed)
        {
            int width = GenVars.UndergroundDesertLocation.Width - 6;
            int height = (int)(Main.maxTilesY / 2f);
            int worldSize = GetWorldSize();

            int innerEllipseYRadius = (int)(height * (worldSize == WorldSizeID.Small ? 0.4f : 0.7f));
            int ellipseCenter = y - innerEllipseYRadius;

            int desertCurrentHeight = GenVars.UndergroundDesertLocation.Y + GenVars.UndergroundDesertLocation.Height;
            int minDesertHeight = Main.maxTilesY - 400;
            int maxDesertHeight = Main.maxTilesY - 200;
            float multiplierAtLessHeight = 1f;
            float multiplierAtMaxHeight = worldSize == WorldSizeID.Small ? 0.5f : 0.7f;
            float multiplier = Utils.Remap(desertCurrentHeight, minDesertHeight, maxDesertHeight, multiplierAtLessHeight, multiplierAtMaxHeight);
            int outerEllipseHeight = (int)(height * multiplier);
            ITDShapes.Ellipse outerEllipse = new(x, ellipseCenter, width / 2, outerEllipseHeight);

            GenVars.structures.AddProtectedStructure(new Rectangle(outerEllipse.Container.X, outerEllipse.Y, outerEllipse.XRadius * 2, outerEllipse.YRadius));

            ITDShapes.Ellipse innerEllipse = new(x, ellipseCenter, width / 2, innerEllipseYRadius);

            List<Rectangle> tunnels = [];
            // main shape gen
            outerEllipse.LoopThroughPoints(p =>
            {
                Tile t = Framing.GetTileSafely(p);
                // if we're on the upper half of the ellipse, don't do anything
                if (p.Y < outerEllipse.Y)
                    return;
                // if there's any regular stone in this area, turn it to dark pyracotta
                if (t.TileType == TileID.Stone)
                    t.TileType = darkPyracotta;
                // if this tile is part of the crescent, place dark pyracotta there
                if (innerEllipse.Contains(p))
                    return;
                t.HasTile = true;
                t.TileType = darkPyracotta;
                if (p.Y < Main.UnderworldLayer)
                    t.WallType = pegmatiteWall;
                // if this tile is under the inner ellipse, try to generate tunnels
                if (p.Y < innerEllipse.Y + innerEllipse.YRadius)
                    return;
                // make sure to add a random chance to not create a tunnel
                if (genRand.NextBool(20))
                {
                    // if this tile is in a tunnel, dont try to create another tunnel
                    // idk why but a positive x doesn't work, it just generates a 1 tile wide vertical line. so restrict tunnel creation to the right side of this ellipse
                    if (p.X < outerEllipse.X)
                        return;
                    int xSize = genRand.Next(100, 250) * -1; //(innerEllipse.X > p.X ? -1 : 1);
                    int width = 5;
                    int segments = genRand.Next(8, 16);

                    Rectangle expectedRect = new(
                    p.X + Math.Min(0, xSize), // leftmost point of tunnel
                    p.Y - width, // rectangle centered around p
                    Math.Abs(xSize), // width       
                    width * 2 // height 
                    );

                    int inflateAmt = 4;
                    expectedRect.Inflate(inflateAmt, inflateAmt);

                    if (tunnels.Any(r => r.Intersects(expectedRect)))
                        return;
                    // tunnel end point (additive)
                    Point dirSize = new(xSize, genRand.Next(-1, 2));

                    Rectangle rect = DigQuadTunnel(p, p + dirSize, 5, segments, 2);
                    rect.Inflate(inflateAmt, inflateAmt);

                    tunnels.Add(rect);
                }
            });

            tunnels.Clear();
            // second loop for pegmatite adding
            outerEllipse.LoopThroughPoints(p =>
            {
                if (innerEllipse.Contains(p))
                    return;
                if (!TileHelpers.EdgeTile(p))
                    return;
                TileRunner(p.X, p.Y, 5, 2, pegmatite);
                //OreRunner(p.X, p.Y, 4, 2, pegmatite);
            });
            // now let's do a little dithering
            int padding = 40;
            ITDShapes.Ellipse ditherEllipse = new(x, ellipseCenter, width / 2 + padding, outerEllipseHeight + padding);
            ditherEllipse.LoopThroughPoints(p =>
            {
                Tile t = Framing.GetTileSafely(p);
                // if we're inside the main ellipse or on the upper half, don't do any dithering
                if (p.Y < outerEllipse.Y || outerEllipse.Contains(p))
                    return;
                float outerDistance = outerEllipse.GetDistanceToEdge(p);
                float ditherDistance = ditherEllipse.GetDistanceToEdge(p);

                float factor = (ditherDistance - outerDistance) / (ditherDistance);

                if (genRand.NextFloat() < factor && t.HasTile)
                {
                    t.TileType = darkPyracotta;
                }
                if (t.TileType == TileID.Sand)
                    t.TileType = darkPyracotta;
                if (t.WallType == WallID.HardenedSand || t.WallType == WallID.Sandstone)
                    t.WallType = pegmatiteWall;
            });
        }
    }
}
