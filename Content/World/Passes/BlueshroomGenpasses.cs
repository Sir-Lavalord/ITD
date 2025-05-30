using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.World.WorldGenUtils;
using Terraria.WorldBuilding;
using ITD.Content.Tiles;
using ITD.Utilities;
using Terraria.DataStructures;

namespace ITD.Content.World.Passes
{
    public sealed class BluesoilPass : ITDGenpass
    {
        public override string Name => "Blueshroom Groves";
        public override double Weight => 100.0;
        public override GenpassOrder Order => new(GenpassOrderType.After, "Lakes");
        public override Point16 SelectOrigin()
        {
            // you would think GenVars.snowMinX[0] and GenVars.snowMaxX[0] would provide the correct values but they only do that if the snow biome is perfectly symmetrical for some reason
            int xTileCandidateLeft = 0;
            bool foundTileClosestToTheLeftAndOnTheSurface = false;
            for (int i = 0; i < Main.maxTilesX && !foundTileClosestToTheLeftAndOnTheSurface; i++)
            {
                for (int j = 0; j < GenVars.snowTop; j++)
                {
                    if (Framing.GetTileSafely(i, j).TileType == TileID.SnowBlock && xTileCandidateLeft < i)
                    {
                        xTileCandidateLeft = i;
                        foundTileClosestToTheLeftAndOnTheSurface = true;
                        break;
                    }
                }
            }
            int xTileCandidateRight = Main.maxTilesX;
            bool foundTileClosestToTheRightAndOnTheSurface = false;
            for (int i = Main.maxTilesX - 1; i >= 0 && !foundTileClosestToTheRightAndOnTheSurface; i--)
            {
                for (int j = 0; j < GenVars.snowTop; j++)
                {
                    if (Framing.GetTileSafely(i, j).TileType == TileID.SnowBlock && xTileCandidateRight > i)
                    {
                        xTileCandidateRight = i;
                        foundTileClosestToTheRightAndOnTheSurface = true;
                        break;
                    }
                }
            }

            return new((int)MathHelper.Lerp(xTileCandidateLeft, xTileCandidateRight, 0.5f), GenVars.snowTop);
        }
        public override void Generate(Point16 selectedOrigin)
        {
            int x = selectedOrigin.X;
            int y = selectedOrigin.Y;
            int seed = WorldGen._genRandSeed;
            int width = (GenVars.snowMaxX[0] - GenVars.snowMinX[0]) / 2;
            int height = Main.maxTilesY / 4;
            ITDShapes.Ellipse ellipse = new(x, y, width, height);
            Rectangle rectangle = ellipse.Container;
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
            ellipse.LoopThroughPoints(p =>
            {
                if (p.Y > GenVars.snowTop)
                {
                    Tile t = Framing.GetTileSafely(p);
                    t.HasTile = false;
                }
            });
            ellipse.LoopThroughPoints(p =>
            {
                int i = p.X;
                int j = p.Y;
                if (!WorldGen.InWorld(i, j) || j < GenVars.snowTop)
                    return;
                float n = noise.GetNoise(i, j);
                float c = noiseForCircles.GetNoise(i, j);
                float subnoise = noiseForSubfrost.GetNoise(i, j);
                if (n > placetilefrom)
                {
                    Tile t = Framing.GetTileSafely(p);
                    t.HasTile = true;
                    t.TileType = TileID.SnowBlock;
                }
                if (c > -0.18f)
                {
                    WorldUtils.Gen(p, new Shapes.Circle(20 + (Main.rand.Next(21) - 10), 3), new Actions.SetTile(TileID.SnowBlock));
                }
                if (subnoise > -0.45f)
                {
                    WorldGen.OreRunner(i, j, 2, 2, (ushort)ModContent.TileType<SubfrostTile>());
                }
            });

            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    Helpers.GrowGrass(i, j, ModContent.TileType<Bluegrass>(), TileID.SnowBlock);
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
                    Helpers.GrowTallGrass(i, j, ModContent.TileType<BluegrassBlades>(), ModContent.TileType<Bluegrass>());
                }
            }
        }
    }
}
