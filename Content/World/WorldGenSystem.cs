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

namespace ITD.Content.World
{
    public class WorldGenSystem : ModSystem
    {
        public static LocalizedText BluesoilPassMessage { get; private set; }
        public static LocalizedText DeepDesertPassMessage { get; private set; }

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
            // test world particle (change particle.canvas in particle type)
            //ParticleSystem.NewParticle(ParticleSystem.ParticleType<TestParticle>(), Main.MouseWorld, Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
            //Point pos = Main.MouseWorld.ToTileCoordinates();
            //Helpers.GrowLongMoss(pos.X, pos.Y, ModContent.TileType<LongBlackMold>());
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
