using ITD.Content.Items.Placeable.Biomes.DeepDesert;
using ITD.Content.Tiles.DeepDesert;
using ITD.Content.Walls.DeepDesert;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using static ITD.Utilities.WorldGenHelpers.Procedural;
using static Terraria.WorldGen;

namespace ITD.Content.World.Passes
{
    public sealed class DeepDesertPass : ITDGenpass
    {
        private static ushort darkPyracotta;
        private static ushort lightPyracotta;
        private static ushort pegmatite;
        private static ushort pegmatiteWall;
        public override string Name => "Deep Desert";
        public override double Weight => 100.0;
        public override GenpassOrder Order => new(GenpassOrderType.After, "Granite");
        public override Point16 SelectOrigin()
        {
            darkPyracotta = (ushort)ModContent.TileType<DioriteTile>();
            lightPyracotta = (ushort)ModContent.TileType<LightPyracottaTile>();
            pegmatite = (ushort)ModContent.TileType<PegmatiteTile>();
            pegmatiteWall = (ushort)ModContent.WallType<PegmatiteWallUnsafe>();

            Rectangle desert = GenVars.UndergroundDesertLocation;
            return new(desert.X + desert.Width / 2, desert.Bottom);
        }
        public override void Generate(Point16 selectedOrigin)
        {
            Rectangle desert = GenVars.UndergroundDesertLocation;

            int x = selectedOrigin.X;

            int width = desert.Width;
            int height = desert.Height;

            int innerEllipseYRadius = height / 2;
            int ellipseCenter = desert.Center.Y;

            int distanceFromDesertBottomToHellTop = Main.UnderworldLayer - desert.Bottom;

            int outerEllipseYRadius = innerEllipseYRadius + (int)(distanceFromDesertBottomToHellTop / 1.5f);
            ITDShapes.Ellipse outerEllipse = new(x, ellipseCenter, width / 2, outerEllipseYRadius);

            GenVars.structures.AddProtectedStructure(new Rectangle(outerEllipse.Container.X, outerEllipse.Y, outerEllipse.XRadius * 2, outerEllipse.YRadius));

            ITDShapes.Ellipse innerEllipse = new(x, ellipseCenter, width / 2, innerEllipseYRadius);

            List<Rectangle> tunnels = [];
            List<Rectangle> sanctuaries = [];
            // main shape gen
            outerEllipse.LoopThroughPoints(p =>
            {
                // if we're on the upper half of the ellipse, don't do anything
                if (p.Y < outerEllipse.Y)
                    return;

                Tile t = Framing.GetTileSafely(p);
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
            });

            // second loop to dig tunnels without them being overwritten, and fill in wall gaps between the desert and the DD
            outerEllipse.LoopThroughPoints(p =>
            {
                // if we're in the bottom half of the ellipse, fill in empty walls with pegmatite
                if (p.Y < outerEllipse.Y)
                    return;
                Tile t = Framing.GetTileSafely(p);
                if (t.WallType == WallID.None)
                    t.WallType = pegmatiteWall;
                // if we're in the crescent and under the inner ellipse, try to gen tunnels
                if (p.Y < innerEllipse.Y + innerEllipse.YRadius || innerEllipse.Contains(p))
                    return;
                // make sure to add a random chance to not create a tunnel
                if (genRand.NextBool(10))
                {
                    // if this tile is in a tunnel, dont try to create another tunnel
                    int xSize = genRand.Next(150, 250) * (innerEllipse.X > p.X ? 1 : -1);
                    int tunWidth = genRand.Next(5, 9);
                    int segments = Math.Abs(xSize) / 10;

                    Rectangle expectedRect = new(
                    p.X + Math.Min(0, xSize), // leftmost point of tunnel
                    p.Y - tunWidth, // rectangle centered around p
                    Math.Abs(xSize), // width       
                    tunWidth * 2 // height 
                    );

                    int inflateAmt = 4;
                    expectedRect.Inflate(inflateAmt, inflateAmt);

                    if (tunnels.Any(r => r.Intersects(expectedRect)))
                        return;
                    // tunnel end point (additive)
                    Point dirSize = new(xSize, genRand.Next(-1, 2));

                    Rectangle rect = DigQuadTunnel(p, p + dirSize, tunWidth, segments, tunWidth / 2);
                    rect.Inflate(inflateAmt, inflateAmt);

                    tunnels.Add(rect);
                }
            });
            // let's create some tunnel connections before clearing them
            for (int i = 0; i < tunnels.Count; i++)
            {
                Rectangle tunnel = tunnels[i];

                int maxDistanceForConnection = 24;

                Point bottom = tunnel.Bottom().ToPoint();

                int maxRealEdgeLookup = 16;

                Point edge = bottom;

                // try to find the real edge of this tunnel
                for (int k = 0; k < maxRealEdgeLookup; k++)
                {
                    edge.Y--;
                    if (!TileHelpers.SolidTile(edge))
                        break;
                }
                edge.Y -= 2;

                for (int j = 0; j < maxDistanceForConnection; j++)
                {
                    Point query = bottom + new Point(genRand.Next(-6, 7), j);
                    Rectangle candidate = tunnels.FirstOrDefault(t => t != tunnel && t.Contains(query), Rectangle.Empty);
                    if (candidate != Rectangle.Empty)
                    {
                        // try to find the real edge of this tunnel as well
                        for (int l = 0; l < maxRealEdgeLookup; l++)
                        {
                            query.Y++;
                            if (!TileHelpers.SolidTile(query))
                                break;
                        }
                        DigQuadTunnel(edge, query + new Point(0, 4), 4, 6, 2, null, false);
                        break;
                    }
                }
            }
            tunnels.Clear();

            // third loop for pegmatite adding
            outerEllipse.LoopThroughPoints(p =>
            {
                if (p.Y < outerEllipse.Y || innerEllipse.Contains(p) || !TileHelpers.EdgeTile(p))
                    return;
                TileRunner(p.X, p.Y, 5, 2, pegmatite);
                //OreRunner(p.X, p.Y, 4, 2, pegmatite);
            });
            // now let's do a little dithering
            int padding = 40;
            ITDShapes.Ellipse ditherEllipse = new(x, ellipseCenter, width / 2 + padding, outerEllipseYRadius + padding);
            ditherEllipse.LoopThroughPoints(p =>
            {
                Tile t = Framing.GetTileSafely(p);
                // if we're inside the main ellipse or on the upper half, don't do any dithering
                if (p.Y < outerEllipse.Y || outerEllipse.Contains(p))
                    return;
                float outerDistance = outerEllipse.GetDistanceToEdge(p);
                float ditherDistance = ditherEllipse.GetDistanceToEdge(p);

                float factor = (ditherDistance - outerDistance) / ditherDistance;

                if (genRand.NextFloat() < factor && t.HasTile)
                {
                    t.TileType = darkPyracotta;
                }
                if (t.TileType == TileID.Sand)
                    t.TileType = darkPyracotta;
                if (t.WallType == WallID.HardenedSand || t.WallType == WallID.Sandstone)
                    t.WallType = pegmatiteWall;
            });

            // now let's add some lines of light pyracotta tiles along the entirety of the desert.
            // we will use digquadtunnel's secret ability here... the fact that you can do more than just dig with it.
            int linesAmount = outerEllipse.YRadius / 20;
            int spacing = outerEllipse.YRadius / linesAmount;
            Rectangle rect = outerEllipse.Container;
            for (int i = 0; i < linesAmount; i++)
            {
                Point origin = new(rect.Left, rect.Top + outerEllipse.YRadius + spacing * i);
                Point end = origin + new Point(rect.Width, genRand.Next(-20, 21));
                DigQuadTunnel(origin, end, 2, 18, 1, p =>
                {
                    Tile t = Framing.GetTileSafely(p);
                    if (t.TileType == darkPyracotta)
                        t.TileType = lightPyracotta;
                });
            }
        }
    }
}
