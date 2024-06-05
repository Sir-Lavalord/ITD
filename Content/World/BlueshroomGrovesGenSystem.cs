using System;
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

namespace ITD.Content.World
{
    public class BlueshroomGrovesGenSystem : ModSystem
    {
        public static LocalizedText BluesoilPassMessage { get; private set; }

        public override void SetStaticDefaults()
        {
            BluesoilPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(BluesoilPassMessage)}"));
        }
        // 4. We use the ModifyWorldGenTasks method to tell the game the order that our world generation code should run
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // 5. We use FindIndex to locate the index of the vanilla world generation task called "Shinies". This ensures our code runs at the correct step.
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));
            if (ShiniesIndex != -1)
            {
                // 6. We register our world generation pass by passing in an instance of our custom GenPass class below. The GenPass class will execute our world generation code.
                tasks.Insert(ShiniesIndex + 1, new BlueshroomGrovesGenPass("Blueshroom Groves", 100f));
            }
        }
        public static bool JustPressed(Keys key)
        {
            return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
        }

        public override void PostUpdateWorld()
        {

            if (JustPressed(Keys.D1))
                TestMethod((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
        }

        private void TestMethod(int x, int y)
        {
            //Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 1, y + 1) * 16, 2, Color.YellowGreen, null);
            //Tile thisTile = Framing.GetTileSafely(x, y);
            //Main.NewText("FrameX: " + thisTile.TileFrameX.ToString() + " FrameY: " + thisTile.TileFrameY.ToString());
            //Helpers.GrowTallBluegrass(x, y);
        }
    }
    public class BlueshroomGrovesGenPass : GenPass
    {
        public BlueshroomGrovesGenPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            Point p = GetGenStartPoint();
            GenFloor(p.X, p.Y, WorldGen._genRandSeed);
        }

        private Point GetGenStartPoint()
        {
            Point iceBiomeStart = new Point(0, 0);
            Point iceBiomeEnd = new Point(0, 0);
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
            return (Vector2.Lerp(iceBiomeStart.ToVector2(),iceBiomeEnd.ToVector2(), 0.5f).ToPoint());
        }

        private void GenFloor(float x, float y, int seed)
        {
            int width = Main.maxTilesX/15;
            int height = width;
            var rectangle = new Rectangle((int)x-(width/2), (int)y-(height/2), width, height);
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
                        //WorldGen.PlaceTile(i, j, ModContent.TileType<BluesoilTile>());
                        WorldGen.PlaceTile(i, j, TileID.SnowBlock);
                    }
                    if (c > -0.18f && inCircle(i, j, rectangle, width))
                    {
                        //WorldUtils.Gen(new Point(i, j), new Shapes.Circle(20 + (Main.rand.Next(21) - 10), 3), new Actions.SetTile((ushort)ModContent.TileType<BluesoilTile>()));
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
                    if (Helpers.AptForTree(i, j, 16))
                    {
                        WorldGen.PlaceObject(i, j, ModContent.TileType<BlueshroomSapling>());
                        WorldGen.GrowTree(i, j);
                    }
                }
            }
            for (int i = rectangle.Left; i <= rectangle.Right; i++)
            {
                for (int j = rectangle.Top; j <= rectangle.Bottom; j++)
                {
                    if (Helpers.TileType(i, j, ModContent.TileType<BlueshroomSapling>()))
                    {
                        WorldGen.KillTile(i, j);
                    }
                    Helpers.GrowTallBluegrass(i, j);
                }
            }
        }
    }
}
