using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class SmoothedCharbleBlockTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileMerge[ModContent.TileType<CharbleBlockTile>()][Type] = true;

            HitSound = SoundID.Tink;
            DustType = DustID.Marble;
            MineResist = 2f;

            AddMapEntry(new Color(185, 196, 219));
        }
    }
}