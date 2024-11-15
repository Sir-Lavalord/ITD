﻿using System.Collections.Generic;
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
            int innerEllipseYRadius = (int)(height * 0.7f);
            int ellipseCenter = y - innerEllipseYRadius;
            ITDShapes.Ellipse outerEllipse = new(x, ellipseCenter, width / 2, height);

            // idk why this fails half of the time but whatever
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
                // if this tile is under the inner ellipse, try to generate tunnels
                if (p.Y < innerEllipse.Y + innerEllipse.YRadius)
                    return;
                // make sure to add a random chance to not create a tunnel
                if (genRand.NextBool(160))
                {
                    // if this tile is in a tunnel, dont try to create another tunnel
                    if (tunnels.Any(r => r.Contains(p)))
                        return;
                    // tunner direction and length
                    Vector2 dirSize = new(Math.Sign(outerEllipse.X - p.X) * genRand.Next(8, 16), genRand.NextFloat(-3f, 3f));

                    tunnels.Add(DigQuadTunnel(p, dirSize, 5, 8, 2));
                }
            });
            // now let's do a little dithering
            int padding = 40;
            ITDShapes.Ellipse ditherEllipse = new(x, ellipseCenter, width / 2 + padding, height + padding);
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
            });
        }
    }
}
