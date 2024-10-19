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
            TileHelpers.VanillaTileFraming(i, j);
            TileHelpers.VanillaTileMergeWithOther(i, j, TileID.Dirt);
            TileHelpers.VanillaTileMergeWithOther(i, j, TileID.Stone, 180, 270);
            TileHelpers.VanillaTileMergeWithOther(i, j, ModContent.TileType<DepthrockTile>(), 180, 270);
            return false;
        }
    }
}