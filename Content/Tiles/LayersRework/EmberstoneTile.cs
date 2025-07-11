using ITD.Utilities;

namespace ITD.Content.Tiles.LayersRework
{
    public class EmberstoneTile : ModTile
    {
        private Asset<Texture2D> glowmask;
        public override void SetStaticDefaults()
        {
            glowmask = ModContent.Request<Texture2D>("ITD/Content/Tiles/LayersRework/EmberstoneTile_Glow");
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            Main.tileMerge[ModContent.TileType<DepthrockTile>()][Type] = true;

            MinPick = 50;
            HitSound = SoundID.Tink;
            DustType = DustID.Stone;

            AddMapEntry(new Color(41, 40, 53));
        }
        public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight)
        {
            WorldGen.TileMergeAttempt(-2, ModContent.TileType<DepthrockTile>(), ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileHelpers.DrawSlopedGlowMask(i, j, glowmask.Value, Color.White, Vector2.Zero);
        }
    }
}
