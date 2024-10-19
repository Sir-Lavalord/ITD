using ITD.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.LayersRework
{
    public class DepthrockTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            Main.tileMerge[TileID.Dirt][ModContent.TileType<DepthrockTile>()] = true;
            Main.tileMerge[ModContent.TileType<ShaleTile>()][ModContent.TileType<DepthrockTile>()] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = DustID.Stone;

            AddMapEntry(new Color(82, 82, 90));
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileHelpers.VanillaTileFraming(i, j);
            TileHelpers.VanillaTileMergeWithOther(i, j, TileID.Dirt);
            TileHelpers.VanillaTileMergeWithOther(i, j, ModContent.TileType<ShaleTile>(), 180, 270);
            return false;
        }
    }
}