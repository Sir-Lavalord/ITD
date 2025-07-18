﻿using ITD.Content.Dusts;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class SubfrostTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMerge[Type][TileID.SnowBlock] = true;
            Main.tileMerge[Type][TileID.IceBlock] = true;
            Main.tileMerge[Type][ModContent.TileType<Bluegrass>()] = true;
            Main.tileMerge[TileID.SnowBlock][Type] = true;
            Main.tileMerge[TileID.IceBlock][Type] = true;
            Main.tileMerge[ModContent.TileType<Bluegrass>()][Type] = true;
            Main.tileBlockLight[Type] = true;

            HitSound = SoundID.Item50;
            DustType = ModContent.DustType<SubfrostDust>();

            AddMapEntry(new Color(168, 217, 255));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void FloorVisuals(Player player)
        {
            player.slippy2 = true;
            player.AddBuff(BuffID.Frostburn, 60);
        }

        public override void RandomUpdate(int i, int j)
        {
            Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, ModContent.DustType<SubfrostDust>());
        }
    }
}