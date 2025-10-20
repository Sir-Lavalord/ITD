using ITD.Content.Dusts;

namespace ITD.Content.Tiles.DeepDesert;

public class PegmatiteTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.ChecksForMerge[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;
        Main.tileMerge[ModContent.TileType<DioriteTile>()][Type] = true;

        MinPick = 55;
        HitSound = SoundID.Tink;
        DustType = ModContent.DustType<PegmatiteDust>();

        AddMapEntry(new Color(153, 105, 103));
    }
    public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
    {
        WorldGen.TileMergeAttempt(-2, ModContent.TileType<DioriteTile>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
    }
}
