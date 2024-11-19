using ITD.Content.Dusts;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Tiles.DeepDesert
{
    public class DioriteTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMerge[ModContent.TileType<LightPyracottaTile>()][Type] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = DustID.Sandstorm;

            AddMapEntry(new Color(199, 108, 91));
        }
        public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
        {
            WorldGen.TileMergeAttempt(-2, ModContent.TileType<LightPyracottaTile>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        }
    }
}