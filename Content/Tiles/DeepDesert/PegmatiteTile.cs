using ITD.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.DeepDesert
{
    public class PegmatiteTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMerge[ModContent.TileType<DioriteTile>()][ModContent.TileType<PegmatiteTile>()] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = DustID.Sandstorm;

            AddMapEntry(new Color(153, 105, 103));
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileHelpers.VanillaTileFraming(i, j);
            TileHelpers.VanillaTileMergeWithOther(i, j, ModContent.TileType<DioriteTile>());
            return false;
        }
    }
}
