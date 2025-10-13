using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ObjectData;

namespace ITD.Content.Tiles;

public abstract class ITDPainting : ModTile
{
    public Color MapColor { get; set; }
    public LocalizedText MapName = Language.GetText("MapObject.Painting");
    public int[] PaintingItem { get; set; }
    public int[] PaintingSize { get; set; } = [2, 2]; // Default 2x2 size
    public int[] PaintingCord { get; set; } = [16, 16]; // Default 16x16 size


    //professional code swiping
    /// <summary>
    /// Override to set DustType, MapColor, MapName (localization for the tile in map, like custom like the catacomb item, the vanilla one), PaintingItem, PaintingSize (tile size) and PaintingCord (for multi-tile tiles)
    /// </summary>
    public virtual void SetStaticPaintingDefaults()
    {
        DustType = DustID.WoodFurniture;
        MapColor = Color.White;
        MapName = Language.GetText("MapObject.Painting");
        PaintingItem = [ItemID.Waldo];
        PaintingSize = [2, 2];
        PaintingCord = [16, 16];
    }
    public sealed override void SetStaticDefaults()
    {
        SetStaticPaintingDefaults();

        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSpelunker[Type] = false;
        Main.tileWaterDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
        TileObjectData.newTile.Width = PaintingSize[0];
        TileObjectData.newTile.Height = PaintingSize[1];
        TileObjectData.newTile.Origin = new Point16(PaintingSize[0] - 1, PaintingSize[1] - 1);
        TileObjectData.newTile.CoordinateHeights = PaintingCord;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.addTile(Type);
        TileID.Sets.FramesOnKillWall[Type] = true;
        AddMapEntry(MapColor, MapName);
        SetStaticDefaultsSafe();
    }
    public virtual void SetStaticDefaultsSafe()
    {

    }
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}
