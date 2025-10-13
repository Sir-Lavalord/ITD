using ITD.Content.Dusts;

namespace ITD.Content.Tiles.BlueshroomGroves;

public class CyaniteOreTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileMerge[Type][TileID.SnowBlock] = true;
        Main.tileMerge[Type][ModContent.TileType<SubfrostTile>()] = true;
        Main.tileMerge[TileID.SnowBlock][Type] = true;
        Main.tileMerge[TileID.IceBlock][ModContent.TileType<SubfrostTile>()] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileSpelunker[Type] = true;
        Main.tileOreFinderPriority[Type] = 660;
        Main.tileShine[Type] = 60;
        MinPick = 190;

        HitSound = SoundID.Tink;
        DustType = ModContent.DustType<SubfrostDust>();

        var name = CreateMapEntryName();
        AddMapEntry(Color.Cyan, name);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}