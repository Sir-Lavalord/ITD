using ITD.Content.Dusts;
using ITD.Content.Tiles.LayersRework;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class CharbleBlockTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileMerge[ModContent.TileType<SmoothedCharbleBlockTile>()][Type] = true;

            HitSound = SoundID.Tink;
            DustType = DustID.Marble;
            MineResist = 2f;

            AddMapEntry(new Color(185, 196, 219));
        }
    }
}