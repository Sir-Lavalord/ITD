﻿namespace ITD.Content.Tiles.LayersRework
{
    public class DepthrockTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileMerge[ModContent.TileType<ShaleTile>()][Type] = true;

            MinPick = 45;
            HitSound = SoundID.Tink;
            DustType = DustID.Stone;

            AddMapEntry(new Color(82, 82, 90));
        }
        public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
        {
            WorldGen.TileMergeAttempt(-2, ModContent.TileType<ShaleTile>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        }
    }
}