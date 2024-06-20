using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles
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
            Tile up = Framing.GetTileSafely(i, j - 1);
            Tile upLeft = Framing.GetTileSafely(i - 1, j - 1);
            Tile upRight = Framing.GetTileSafely(i + 1, j - 1);
            Tile right = Framing.GetTileSafely(i + 1, j);
            Tile left = Framing.GetTileSafely(i - 1, j);
            Tile down = Framing.GetTileSafely(i, j + 1);
            Tile downLeft = Framing.GetTileSafely(i - 1, j + 1);
            Tile downRight = Framing.GetTileSafely(i + 1, j + 1);
            Tile thisTile = Framing.GetTileSafely(i, j);
            Helpers.VanillaTileFraming(ref thisTile, up, upLeft, upRight, down, downLeft, downRight, left, right);
            Helpers.VanillaTileMergeWithOther(ref thisTile, ModContent.TileType<DioriteTile>(), up, upLeft, upRight, down, downLeft, downRight, left, right);
            return false;
        }
    }
}
