using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.World.WorldGenUtils;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria;
using Microsoft.Xna.Framework;
using ITD.Content.Tiles;
using ITD.Utilities;

namespace ITD.Content.World
{
    public class BlueshroomGrovesGenPass(string name, float loadWeight) : GenPass(name, loadWeight)
    {
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            Point p = GetGenStartPoint();
            GenFloor(p.X, p.Y, WorldGen._genRandSeed);
        }

        private static Point GetGenStartPoint()
        {
            Point iceBiomeStart = new(0, 0);
            Point iceBiomeEnd = new(0, 0);
            for (int i = 0; i < Main.maxTilesX && iceBiomeStart == Point.Zero; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (Main.tile[i, j].TileType == TileID.IceBlock)
                    {
                        iceBiomeStart = new Point(i, j);
                        break;
                    }
                }
            }
            for (int i = Main.maxTilesX; i >= 0 && iceBiomeEnd == Point.Zero; i--)
            {
                for (int j = Main.maxTilesY; j >= 0; j--)
                {
                    if (Main.tile[i, j].TileType == TileID.IceBlock)
                    {
                        iceBiomeEnd = new Point(i, j);
                        break;
                    }
                }
            }
            return (Vector2.Lerp(iceBiomeStart.ToVector2(), iceBiomeEnd.ToVector2(), 0.5f).ToPoint());
        }

        private static void GenFloor(float x, float y, int seed)
        {
            int width = Main.maxTilesX / 15;
            int height = width;
            var rectangle = new Rectangle((int)x - (width / 2), (int)y - (height / 2), width, height);
            if (rectangle.Y < 400)
            {
                rectangle.Y = 400;
            }
            if (rectangle.Y > Main.maxTilesY / 2.1f)
            {
                rectangle.Y = (int)(Main.maxTilesY / 2.1f);
            }
            var placetilefrom = -0.58f;
            var noise = new FastNoise
            {
                Seed = seed,
                NoiseType = FastNoise.NoiseTypes.CubicFractal,
                Frequency = 0.06f,
                FractalOctaves = 3,
                FractalType = FastNoise.FractalTypes.Billow,
                FractalGain = 0.25f,
                FractalLacunarity = 2.4f
            };
            var noiseForCircles = new FastNoise
            {
                Seed = seed,
                NoiseType = FastNoise.NoiseTypes.CubicFractal,
                Frequency = 0.06f,
                FractalOctaves = 3,
                FractalType = FastNoise.FractalTypes.Billow,
                FractalGain = 0.25f,
                FractalLacunarity = 2.4f
            };
            var noiseForSubfrost = new FastNoise
            {
                Seed = seed,
                NoiseType = FastNoise.NoiseTypes.CubicFractal,
                Frequency = 0.15f,
                FractalOctaves = 3,
                FractalType = FastNoise.FractalTypes.Billow,
                FractalGain = 0.2f,
                FractalLacunarity = 4f
            };
            static bool inCircle(int i, int j, Rectangle rect, int wid)
            {
                Point rC = rect.Center;
                int xElem = i - rC.X;
                int yElem = j - rC.Y;
                int r = wid / 2;
                return xElem * xElem + yElem * yElem <= r * r;
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    if (inCircle(i, j, rectangle, width))
                    {
                        WorldGen.digTunnel(i, j, 0, 0, 1, 1, false);
                    }
                }
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {

                    float n = noise.GetNoise(i, j);
                    float c = noiseForCircles.GetNoise(i, j);
                    float subnoise = noiseForSubfrost.GetNoise(i, j);
                    if (n > placetilefrom)
                    {
                        WorldGen.PlaceTile(i, j, TileID.SnowBlock);
                    }
                    if (c > -0.18f && inCircle(i, j, rectangle, width))
                    {
                        WorldUtils.Gen(new Point(i, j), new Shapes.Circle(20 + (Main.rand.Next(21) - 10), 3), new Actions.SetTile((ushort)TileID.SnowBlock));
                    }
                    if (subnoise > -0.45f && inCircle(i, j, rectangle, width))
                    {
                        WorldGen.OreRunner(i, j, 2, 2, (ushort)ModContent.TileType<SubfrostTile>());
                    }
                }
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    Helpers.GrowBluegrass(i, j);
                }
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    if (TileHelpers.AptForTree(i, j, 16))
                    {
                        ITDTree.Grow(i, j, 0, 8, 14);
                    }
                }
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    Helpers.GrowTallBluegrass(i, j);
                }
            }
        }
    }
}
