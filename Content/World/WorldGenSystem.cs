using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using Terraria.ID;
using Microsoft.Xna.Framework;
using ITD.Physics;
using ITD.Particles;
using ITD.Content.Tiles.Misc;
using ITD.Utilities;
using Terraria.ObjectData;
using ITD.Particles.CosJel;
using ITD.Content.Tiles.DeepDesert;
using ITD.Content.Walls.DeepDesert;
using System;
using ITD.Content.UI;
using ITD.Content.Events;

namespace ITD.Content.World
{
    public class WorldGenSystem : ModSystem
    {
        public static LocalizedText BluesoilPassMessage { get; private set; }
        public static LocalizedText DeepDesertPassMessage { get; private set; }
        public static LocalizedText WorldNPCsPassMessage { get; private set; }

        public static List<Point> testPoints = [];
        public override void SetStaticDefaults()
        {
            BluesoilPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(BluesoilPassMessage)}"));
            DeepDesertPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(DeepDesertPassMessage)}"));
            WorldNPCsPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(WorldNPCsPassMessage)}"));
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int blueshroomIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));
            int deepdesertIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Granite"));
            int worldNPCsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Guide"));
            if (blueshroomIndex != -1)
            {
                tasks.Insert(blueshroomIndex + 1, new BlueshroomGrovesGenPass("Blueshroom Groves", 100f));
            }
            if (deepdesertIndex != -1)
            {
                tasks.Insert(deepdesertIndex + 1, new DeepDesertGenPass("Deep Desert", 100f));
            }
            if (worldNPCsIndex == -1)
                worldNPCsIndex = tasks.Count - 1;
            tasks.Insert(worldNPCsIndex + 1, new SpawnWorldNPCsGenpass("World NPCs", 0.016f));
        }
        public static bool JustPressed(Keys key)
        {
            return Main.keyState.IsKeyDown(key) && !Main.oldKeyState.IsKeyDown(key);
        }

        public override void PostUpdateWorld()
        {
            if (JustPressed(Keys.D1))
            {
                TestMethod();
            }
            if (JustPressed(Keys.D2))
            {
                TestMethod2();
            }
        }

        private void TestMethod()
        {
            // test UI particle (change particle.canvas in particle type)
            //ParticleSystem.NewEmitter(ParticleSystem.ParticleType<TestParticle>(), Main.MouseScreen/Main.UIScale, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
            //test world particle (change particle.canvas in particle type)
            //ITDParticle newParticle = ParticleSystem.NewEmitter<TestParticle>(Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f, 0f);
            Point pos = Main.MouseWorld.ToTileCoordinates();
            //EventsSystem.BeginEvent<TestChaoticEvent>();
            /*
            Tile t = Framing.GetTileSafely(pos);
            t.TileType = (ushort)ModContent.TileType<ReinforcedPegmatiteBricks>();
            t.HasTile = true;
            */
            /*
            testPoints.Add(pos);
            if (testPoints.Count == 1)
                UILoader.GetUIState<WorldNPCDialogue>().Open("Mudkarp.FirstTimeSpeaking");
            else
            {
                UILoader.GetUIState<WorldNPCDialogue>().Close();
                testPoints.Clear();
            }
            */

            /*
            testPoints.Add(pos);
            if (testPoints.Count == 2)
            {
                Point p1 = testPoints[0];
                Point p2 = testPoints[1];
                ITDShapes.Ellipse ellipse = new(p1.X, p1.Y, Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
                ellipse.LoopThroughPoints(p =>
                {
                    Dust d = Dust.NewDustDirect(p.ToWorldCoordinates(), 1, 1, DustID.Torch);
                    d.noGravity = true;
                });
                Point rand = ellipse.RandomPoint(WorldGen.genRand);
                Dust.DrawDebugBox(new Rectangle(rand.X * 16, rand.Y * 16, 16, 16));
                testPoints.Clear();
            }
            */
            /*
            testPoints.Add(pos);
            if (testPoints.Count == 2)
            {
                Point p1 = testPoints[0];
                Point p2 = testPoints[1];
                Rectangle rect = MiscHelpers.ContainsRectangles(new Rectangle(p1.X, p1.Y, 1, 1), new Rectangle(p2.X, p2.Y, 1, 1));
                for (int i = rect.Left; i < rect.Right; i++)
                {
                    for (int j = rect.Top; j < rect.Bottom; j++)
                    {
                        Framing.GetTileSafely(i, j).WallType = (ushort)ModContent.WallType<ReinforcedPegmatiteBrickWallUnsafe>();
                        WorldGen.SquareWallFrame(i, j);
                    }
                }
                testPoints.Clear();
            }
            */
            /*
            Tile t = Framing.GetTileSafely(pos);
            t.HasTile = true;
            t.TileType = (ushort)ModContent.TileType<ReinforcedPegmatiteBricks>();
            WorldGen.TileFrame(pos.X, pos.Y);
            */
            //WorldGenHelpers.Procedural.DigQuadTunnel(pos, pos + new Point(100, 50), 5, 9, 3);
            /*
            Point dirSize = new(
                    WorldGen.genRand.Next(35, 50) * (1),
                    -1
                    );
            Rectangle rect = WorldGenHelpers.Procedural.DigQuadTunnel(pos, pos + dirSize, 5, 8, 2);
            Rectangle newRect = new(rect.X * 16, rect.Y * 16, rect.Width * 16, rect.Height * 16);
            Dust.DrawDebugBox(newRect);
            WorldGenHelpers.QuickDebugRectangle(rect);
            */
            /*
            testPoints.Add(pos);
            if (testPoints.Count == 2)
            {
                Point origin = testPoints[0];
                Vector2 sizeDir = (testPoints[1] - testPoints[0]).ToVector2();
                //WorldGenHelpers.Procedural.DigDirectionQuad(origin, sizeDir, 6, 6, 4, true);
                WorldGenHelpers.Procedural.DigQuadTunnel(origin, sizeDir, 6, 3, 3);
                testPoints.Clear();
            }
            */
            /*
            testPoints.Add(pos);
            if (testPoints.Count == 2)
            {
                ITDShapes.Parabola par = new(testPoints[0].X, testPoints[0].Y, testPoints[1].X, testPoints[1].Y, 50);
                
                //par.LoopThroughPoints(p =>
                //{
                //    WorldGen.PlaceTile(p.X, p.Y, TileID.BlueDungeonBrick);
                //});
                
                double tightness = 0.9d;
                ITDShapes.Banana ban = new(par, testPoints[1].Y, tightness);
                ban.LoopThroughPoints(p =>
                {
                    WorldGen.PlaceTile(p.X, p.Y, TileID.BlueDungeonBrick);
                });
                testPoints.Clear();
            }
            */
            /* test triangle creation
            testPoints.Add(pos);
            if (testPoints.Count == 3)
            {
                ITDShapes.Triangle tri = new(testPoints[0], testPoints[1], testPoints[2]);
                tri.LoopThroughPoints(p =>
                {
                    WorldGen.PlaceTile(p.X, p.Y, TileID.BlueDungeonBrick);
                });
                testPoints.Clear();
            }
            */
            //Helpers.GrowLongMoss(pos.X, pos.Y, ModContent.TileType<LongBlackMold>(), ModContent.TileType<BlackMold>());
            //WorldGen.PlaceTile(pos.X, pos.Y, ModContent.TileType<BlackMold>());
        }
        private void TestMethod2()
        {
            //EventsSystem.CancelEvent<TestChaoticEvent>();
            // test UI particle (change particle.canvas in particle type)
            //ParticleSystem.NewEmitter(ParticleSystem.ParticleType<ShaderTestParticle>(), Main.MouseScreen/Main.UIScale, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
            // test world particle (change particle.canvas in particle type)
            //ParticleSystem.NewEmitter(ParticleSystem.ParticleType<ShaderTestParticle>(), Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
        }
    }
}
