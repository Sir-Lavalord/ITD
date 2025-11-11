namespace ITD.Systems.DataStructures;

public struct TinyTile(ref Tile t)
{
    private const int HasTileI = 0;
    private const int IsTileInvisibleI = 1;
    private const int IsTileFullbrightI = 2;
    private const int IsWallInvisibleI = 3;
    private const int IsWallFullbrightI = 4;
    private const int IsActuatedI = 5;
    private const int HasActuatorI = 6;
    public ushort TileType = t.TileType;
    public BlockType BlockType = t.BlockType;
    public BitsByte BasicFlags = 
    new
    (
        t.HasTile,
        t.IsTileInvisible,
        t.IsTileFullbright,
        t.IsWallInvisible,
        t.IsWallFullbright,
        t.IsActuated,
        t.HasActuator
    );
    public readonly bool HasTile => BasicFlags[HasTileI];
    public readonly bool IsTileInvisible => BasicFlags[IsTileInvisibleI];
    public readonly bool IsTileFullbright => BasicFlags[IsTileFullbrightI];
    public readonly bool IsWallInvisible => BasicFlags[IsWallInvisibleI];
    public readonly bool IsWallFullbright => BasicFlags[IsWallFullbrightI];
    public readonly bool IsActuated => BasicFlags[IsActuatedI];
    public readonly bool HasActuator => BasicFlags[HasActuatorI];
    public byte LiquidAmount = t.LiquidAmount;
    public int LiquidType = t.LiquidType;
    public int TileFrameNumber = t.TileFrameNumber;
    public short TileFrameX = t.TileFrameX;
    public short TileFrameY = t.TileFrameY;
    public ushort WallType = t.WallType;
    public byte WallColor = t.WallColor;
    public byte TileColor = t.TileColor;
    public int WallFrameNumber = t.WallFrameNumber;
    public int WallFrameX = t.WallFrameX;
    public int WallFrameY = t.WallFrameY;
    public WireType Wires = GetWires(ref t);
    public readonly bool RedWire => (Wires & WireType.Red) != 0;
    public readonly bool BlueWire => (Wires & WireType.Blue) != 0;
    public readonly bool GreenWire => (Wires & WireType.Green) != 0;
    public readonly bool YellowWire => (Wires & WireType.Yellow) != 0;
    public static WireType GetWires(ref Tile t)
    {
        WireType final = WireType.None;
        if (t.RedWire)
            final |= WireType.Red;
        if (t.BlueWire)
            final |= WireType.Blue;
        if (t.GreenWire)
            final |= WireType.Green;
        if (t.YellowWire)
            final |= WireType.Yellow;
        return final;
    }
    public readonly void CopyTo(ref Tile t)
    {
        CopyWallTo(ref t);
        CopyTileTo(ref t);
        CopyLiquidTo(ref t);
        CopyWiringTo(ref t);
    }
    public readonly void CopyWallTo(ref Tile t)
    {
        t.WallType = WallType;
        t.WallColor = WallColor;
        t.IsWallInvisible = IsWallInvisible;
        t.IsWallFullbright = IsWallFullbright;
        t.WallFrameNumber = WallFrameNumber;
        t.WallFrameX = WallFrameX;
        t.WallFrameY = WallFrameY;
    }
    public readonly void CopyTileTo(ref Tile t)
    {
        t.HasTile = HasTile;
        t.TileType = TileType;
        t.TileColor = TileColor;
        t.BlockType = BlockType;
        t.IsTileInvisible = IsTileInvisible;
        t.IsTileFullbright = IsTileFullbright;
        t.TileFrameNumber = TileFrameNumber;
        t.TileFrameX = TileFrameX;
        t.TileFrameY = TileFrameY;
        t.IsActuated = IsActuated;
    }
    public readonly void CopyLiquidTo(ref Tile t)
    {
        t.LiquidType = LiquidType;
        t.LiquidAmount = LiquidAmount;
    }
    public readonly void CopyWiringTo(ref Tile t)
    {
        t.RedWire = (Wires & WireType.Red) != 0;
        t.BlueWire = (Wires & WireType.Blue) != 0;
        t.GreenWire = (Wires & WireType.Green) != 0;
        t.YellowWire = (Wires & WireType.Yellow) != 0;
        t.HasActuator = HasActuator;
    }
}

public enum WireType : byte
{
    None = 0,
    Red = 1,
    Blue = 2,
    Green = 4,
    Yellow = 8,
}
