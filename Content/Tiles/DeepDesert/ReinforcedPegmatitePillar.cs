using System.Runtime.CompilerServices;

namespace ITD.Content.Tiles.DeepDesert;

public class ReinforcedPegmatitePillar : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
        TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    public override bool CanExplode(int i, int j) => false;
    public override bool KillSound(int i, int j, bool fail) => false;
    public override bool CreateDust(int i, int j, ref int type) => false;
    private static void FrameToPoint(Tile t, short x, short y)
    {
        t.TileFrameX = x;
        t.TileFrameY = y;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftLeftTile(int i, int j) => IsLeftLeftTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftLeftTile(Tile t) => t.TileFrameX == 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightRightTile(int i, int j) => IsRightRightTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightRightTile(Tile t) => t.TileFrameX == 72;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightTile(int i, int j) => IsRightTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightTile(Tile t) => t.TileFrameX == 54;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftTile(int i, int j) => IsLeftTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftTile(Tile t) => t.TileFrameX == 18;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCenterTile(int i, int j) => IsCenterTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCenterTile(Tile t) => t.TileFrameX == 36;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCenterBottomTile(int i, int j) => IsCenterBottomTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCenterBottomTile(Tile t) => IsCenterTile(t) && t.TileFrameY == 36;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCenterTopTile(int i, int j) => IsCenterTopTile(Framing.GetTileSafely(i, j));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCenterTopTile(Tile t) => IsCenterTile(t) && t.TileFrameY == 0;
    public static Point GetPillarBottom(int i, int j)
    {
        if (IsCenterBottomTile(i, j))
            return new(i, j);
        if (IsCenterTile(i, j))
        {
            while (!IsCenterBottomTile(i, j))
                j++;
            return new(i, j);
        }
        if (IsRightRightTile(i, j))
            return GetPillarBottom(i - 2, j);
        if (IsRightTile(i, j))
            return GetPillarBottom(i - 1, j);
        if (IsLeftLeftTile(i, j))
            return GetPillarBottom(i + 2, j);
        if (IsLeftTile(i, j))
            return GetPillarBottom(i + 1, j);
        Main.NewText("If you're seeing this I did something wrong, send screenshot of the offending pillar in the Discord pls");
        Main.NewText("- Q", new Color(255, 153, 204));
        return new(i, j);
    }
    public static int GetPillarHeight(Point bottom)
    {
        int h = 1;
        while (!IsCenterTopTile(bottom.X, bottom.Y))
        {
            bottom.Y--;
            h++;
        }
        return h;
    }
    public static void Destroy(Point p) => Destroy(p.X, p.Y);
    public static void Destroy(int i, int j)
    {
        Point bottom = GetPillarBottom(i, j);
        int height = GetPillarHeight(bottom);
        for (int k = 0; k < height; k++)
        {
            if (k == 0)
            {
                TileHelpers.KillTilesForced(bottom.X - 2, bottom.Y, 5);
            }
            else if (k == height - 1)
            {
                TileHelpers.KillTilesForced(bottom.X - 2, bottom.Y - k, 5);
            }
            else
            {
                TileHelpers.KillTilesForced(bottom.X - 1, bottom.Y - k, 3);
            }
        }
    }
    public static bool Generate(int i, int j, int height)
    {
        int type = ModContent.TileType<ReinforcedPegmatitePillar>();
        if (!TileHelpers.AptForTree(i, j, height - 1))
        {
            Main.NewText("Couldn't generate pillar at the given coordinates: There's not enough space", Color.Red);
            return false;
        }
        for (int k = 0; k < height; k++)
        {
            WorldGen.PlaceTile(i, j - k, type);
            WorldGen.PlaceTile(i - 1, j - k, type);
            WorldGen.PlaceTile(i + 1, j - k, type);
            Tile centerTile = Framing.GetTileSafely(i, j - k);
            Tile leftTile = Framing.GetTileSafely(i - 1, j - k);
            Tile rightTile = Framing.GetTileSafely(i + 1, j - k);
            if (k == 0)
            {
                WorldGen.PlaceTile(i - 2, j, type);
                WorldGen.PlaceTile(i + 2, j, type);
                Tile leftLeftTile = Framing.GetTileSafely(i - 2, j);
                Tile rightRightTile = Framing.GetTileSafely(i + 2, j);

                leftLeftTile.FrameToPoint(0, 36);
                leftTile.FrameToPoint(18, 36);
                centerTile.FrameToPoint(36, 36);
                rightTile.FrameToPoint(54, 36);
                rightRightTile.FrameToPoint(72, 36);

                TileHelpers.Sync(i - 2, j, 5);
            }
            else if (k == height - 1)
            {
                WorldGen.PlaceTile(i - 2, j - k, type);
                WorldGen.PlaceTile(i + 2, j - k, type);
                Tile leftLeftTile = Framing.GetTileSafely(i - 2, j - k);
                Tile rightRightTile = Framing.GetTileSafely(i + 2, j - k);

                leftLeftTile.FrameToPoint(0);
                leftTile.FrameToPoint(18);
                centerTile.FrameToPoint(36);
                rightTile.FrameToPoint(54);
                rightRightTile.FrameToPoint(72);

                TileHelpers.Sync(i - 2, j - k, 5);
            }
            else
            {
                leftTile.FrameToPoint(18, 18);
                centerTile.FrameToPoint(36, 18);
                rightTile.FrameToPoint(54, 18);
                TileHelpers.Sync(i - 1, j - k, 3);
            }
        }
        return true;
    }
}
