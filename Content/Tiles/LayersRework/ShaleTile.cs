using ITD.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.LayersRework
{
    public class ShaleTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            Main.tileMerge[TileID.Dirt][ModContent.TileType<ShaleTile>()] = true;
            Main.tileMerge[TileID.Stone][ModContent.TileType<ShaleTile>()] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = DustID.Stone;

            AddMapEntry(new Color(92, 92, 92));
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
            Helpers.VanillaTileMergeWithOther(ref thisTile, TileID.Dirt, up, upLeft, upRight, down, downLeft, downRight, left, right);
            Helpers.VanillaTileMergeWithOther(ref thisTile, TileID.Stone, up, upLeft, upRight, down, downLeft, downRight, left, right, 180, 270);
            Helpers.VanillaTileMergeWithOther(ref thisTile, ModContent.TileType<DepthrockTile>(), up, upLeft, upRight, down, downLeft, downRight, left, right, 180, 270);
            return false;
        }
    }
}