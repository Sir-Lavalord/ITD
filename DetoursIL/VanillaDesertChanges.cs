using Daybreak.Common.Features.Hooks;
using ReLogic.Utilities;
using Terraria.GameContent.Biomes.Desert;

namespace ITD.DetoursIL;

public static class DesertChanges
{

    [OnLoad]
    public static void Load()
    {
        // we detouring obscure ahh methods with this one
        // i was originally going to IL this but the method's so short a detour suits it better i think
        On_DesertDescription.CreateFromPlacement += ModifyDesertHeight;
    }
    private static DesertDescription ModifyDesertHeight(On_DesertDescription.orig_CreateFromPlacement orig, Point origin)
    {
        // call orig over here because yeah
        if (!ITD.ServerConfig.ResizeDesertForDeepDesert)
            return orig(origin);
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

        if (DesertDescription.RowHasInvalidTiles(origin.X, surfaceMap.Bottom, desertWidth))
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

        DesertDescription d = new();

        Rectangle combArea = new(origin.X, averageSurfaceHeight, desertWidth, origin.Y + desertHeight - averageSurfaceHeight);
        Rectangle hive = new(origin.X, origin.Y + hiveOffset, desertWidth, desertHeight - hiveOffset);
        Rectangle desert = new(origin.X, averageSurfaceHeight, desertWidth, origin.Y + desertHeight / 2 - averageSurfaceHeight + hiveOffset);

        d.CombinedArea = combArea;
        d.Hive = hive;
        d.Desert = desert;
        d.BlockScale = blockScale;
        d.BlockColumnCount = blockColumns;
        d.BlockRowCount = blockRows;
        d.Surface = surfaceMap;
        d.IsValid = true;

        return d;
    }
}
