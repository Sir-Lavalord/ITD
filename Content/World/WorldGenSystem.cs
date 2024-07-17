using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using Terraria.ID;
using Microsoft.Xna.Framework;

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
            /*
            if (Main.keyState.IsKeyDown(Keys.D1))
            {
                Dust d = Dust.NewDustPerfect(Helpers.QuickRaycast(Main.LocalPlayer.Center, Main.MouseWorld - Main.LocalPlayer.Center, 12f), DustID.Torch);
                d.noGravity = true;
                d.velocity = Vector2.Zero;
            }
            */
        }

        private void TestMethod(int x, int y)
        {
            //Dust.QuickBox(new Vector2(x, y) * 16, new Vector2(x + 1, y + 1) * 16, 2, Color.YellowGreen, null);
            Tile thisTile = Framing.GetTileSafely(x, y); 
            Main.NewText(thisTile.TileFrameX.ToString() + " " + thisTile.TileFrameY.ToString());
            //Helpers.GrowTallBluegrass(x, y);
        }
    }
}
