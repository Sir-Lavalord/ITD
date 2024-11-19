using ITD.Utilities;
using MonoMod.Cil;
using ReLogic.Utilities;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Biomes.Desert;
using System;

namespace ITD.DetoursIL
{
    public class DesertChanges : DetourGroup
    {
        public override void Load()
        {
            // we detouring obscure ahh methods with this one
            // i was originally going to IL this but the method's so short a detour suits it better i think
            // ironically, this may be messier than just an IL edit since i have to use reflection in so many places (why tf are most of these things private)
            On_DesertDescription.CreateFromPlacement += ModifyDesertHeight;
        }
        private static DesertDescription ModifyDesertHeight(On_DesertDescription.orig_CreateFromPlacement orig, Point origin)
        {
            // credits to chatgpt for the deobfuscation and for the record this is the only thing i advocate using chatgpt for
            int ScanPadding = 5;

            Vector2D blockScale = new(4.0, 2.0);
            double worldScale = Main.maxTilesX / 4200.0;

            // Calculate block dimensions based on world size and scale
            // adjust this float to multiply the final height
            float finalHeightMultiplier = 0.75f;
            int blockColumns = (int)(80.0 * worldScale);
            int blockRows = (int)((WorldGen.genRand.NextDouble() * 0.5 + 1.5) * 170.0 * worldScale * finalHeightMultiplier);

            if (WorldGen.remixWorldGen)
            {
                blockRows = (int)(340.0 * worldScale);
            }

            int desertWidth = (int)(blockScale.X * blockColumns);
            int desertHeight = (int)(blockScale.Y * blockRows);

            // Adjust origin position
            origin.X -= desertWidth / 2;
            SurfaceMap surfaceMap = SurfaceMap.FromArea(origin.X - ScanPadding, desertWidth + ScanPadding * 2);

            if ((bool)ReflectionHelpers.CallMethod("RowHasInvalidTiles", null, typeof(DesertDescription), BindingFlags.Static | BindingFlags.NonPublic, origin.X, surfaceMap.Bottom, desertWidth))
            {
                return DesertDescription.Invalid;
            }

            // Calculate vertical placement
            int averageSurfaceHeight = (int)(surfaceMap.Average + surfaceMap.Bottom) / 2;
            origin.Y = averageSurfaceHeight + WorldGen.genRand.Next(40, 60);

            int hiveOffset = 0;
            if (Main.tenthAnniversaryWorld)
            {
                hiveOffset = (int)(20.0 * worldScale);
            }

            // ew
            DesertDescription d = (DesertDescription)typeof(DesertDescription).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes).Invoke(null);
            BindingFlags lookup = BindingFlags.Public | BindingFlags.Instance;
            Rectangle combArea = new(origin.X, averageSurfaceHeight, desertWidth, origin.Y + desertHeight - averageSurfaceHeight);
            Rectangle hive = new(origin.X, origin.Y + hiveOffset, desertWidth, desertHeight - hiveOffset);
            Rectangle desert = new(origin.X, averageSurfaceHeight, desertWidth, origin.Y + desertHeight / 2 - averageSurfaceHeight + hiveOffset);
            // brother ew
            ReflectionHelpers.Set<PropertyInfo>("CombinedArea", combArea, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("Hive", hive, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("Desert", desert, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("BlockScale", blockScale, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("BlockColumnCount", blockColumns, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("BlockRowCount", blockRows, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("Surface", surfaceMap, d, null, lookup);
            ReflectionHelpers.Set<PropertyInfo>("IsValid", true, d, null, lookup);

            return d;
            // never call orig because why would you
        }
    }
}
