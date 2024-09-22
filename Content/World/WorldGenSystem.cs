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
                TestMethod((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y);
            }
        }

        private void TestMethod(int x, int y)
        {
            ParticleSystem.NewParticle(ParticleSystem.ParticleType<TestParticle>(), new Vector2(x, y), Main.rand.NextVector2Unit(-MathHelper.PiOver2 - MathHelper.PiOver4, MathHelper.PiOver2) * 6f);
            //Main.LocalPlayer.Center = new Vector2(x, y);
            //Rain.NewRainForced(new Vector2(x, y), Vector2.UnitY * 16f + Vector2.UnitX * 8f);
            //Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 1, y + 1) * 16, 2, Color.YellowGreen, null);
            //PhysicsMethods.CreateVerletChain(8, 16, Main.MouseWorld, Main.MouseWorld + Vector2.One, false);
            //Helpers.GrowTallBluegrass(x, y);
        }
    }
}
