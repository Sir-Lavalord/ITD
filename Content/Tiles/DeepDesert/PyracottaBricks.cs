using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Dusts;

namespace ITD.Content.Tiles.DeepDesert
{
    public class LightPyracottaBricks : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[ModContent.TileType<LightPyracottaTile>()][Type] = true;
            Main.tileMerge[TileID.Sand][Type] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = ModContent.DustType<PyracottaDust>();

            AddMapEntry(new Color(196, 162, 126));
        }
    }
    public class DarkPyracottaBricks : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileMerge[ModContent.TileType<DioriteTile>()][Type] = true;
            Main.tileMerge[TileID.Sand][Type] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = ModContent.DustType<PyracottaDust>();

            AddMapEntry(new Color(191, 88, 65));
        }
    }
}
