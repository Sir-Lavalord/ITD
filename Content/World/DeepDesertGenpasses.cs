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

namespace ITD.Content.World
{
    public class DeepDesertGenPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        private static int diorite;
        private static int pegmatite;
        private static int pegmatiteWall;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            diorite = ModContent.TileType<DioriteTile>();
            pegmatite = ModContent.TileType<PegmatiteTile>();
            pegmatiteWall = ModContent.WallType<PegmatiteWallUnsafe>();
            Point p = GetGenStartPoint();
            GenDeepDesert(p.X, p.Y, WorldGen._genRandSeed);
        }

        private static Point GetGenStartPoint()
        {
            return GenVars.UndergroundDesertLocation.Bottom().ToPoint();
        }

        private static void GenDeepDesert(int x, int y, int seed)
        {
            int width = Main.maxTilesX / 15;
            int height = Main.maxTilesY / 6;
            var rectangle = new Rectangle((int)x - (width / 2), (int)y - (height / 2), width, height);
            ITDShapes.Parabola parab = new(x, y, x, y - 100, 300);
            ITDShapes.Banana banan = new(parab, y - 200, 0.7d);
            banan.LoopThroughPoints(p =>
            {

            });
        }
    }
}
