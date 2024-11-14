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
using ITD.Particles.Testing;
using ITD.Content.Tiles.Misc;
using ITD.Utilities;
using Terraria.ObjectData;
using ITD.Particles.CosJel;

namespace ITD.Content.World
{
    public class WorldGenSystem : ModSystem
    {
        public static LocalizedText BluesoilPassMessage { get; private set; }
        public static LocalizedText DeepDesertPassMessage { get; private set; }

        public static List<Point> testPoints = [];
        public override void SetStaticDefaults()
        {
            BluesoilPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(BluesoilPassMessage)}"));
            DeepDesertPassMessage = Language.GetOrRegister(Mod.GetLocalizationKey($"WorldGen.{nameof(DeepDesertPassMessage)}"));
        }
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int blueshroomIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));
            int deepdesertIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Full Desert"));
            if (blueshroomIndex != -1)
            {
                tasks.Insert(blueshroomIndex + 1, new BlueshroomGrovesGenPass("Blueshroom Groves", 100f));
            }
            if (deepdesertIndex != -1)
            {
                tasks.Insert(deepdesertIndex + 1, new DeepDesertGenPass("Deep Desert", 100f));
            }
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
            //ParticleSystem.NewParticle(ParticleSystem.ParticleType<TestParticle>(), Main.MouseScreen/Main.UIScale, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
            //test world particle (change particle.canvas in particle type)
            //ITDParticle newParticle = ParticleSystem.NewParticle<TestParticle>(Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f, 0f);
            Point pos = Main.MouseWorld.ToTileCoordinates();
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
            // test UI particle (change particle.canvas in particle type)
            //ParticleSystem.NewParticle(ParticleSystem.ParticleType<ShaderTestParticle>(), Main.MouseScreen/Main.UIScale, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
            // test world particle (change particle.canvas in particle type)
            //ParticleSystem.NewParticle(ParticleSystem.ParticleType<ShaderTestParticle>(), Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
        }
    }
}
