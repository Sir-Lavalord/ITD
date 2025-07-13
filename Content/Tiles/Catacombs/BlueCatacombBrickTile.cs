using ITD.Content.Dusts;
using ITD.Utilities;

namespace ITD.Content.Tiles.Catacombs
{
    public class BlueCatacombBrickTile : ModTile
    {
        private Asset<Texture2D> glowmask;
        public override void SetStaticDefaults()
        {
            glowmask = ModContent.Request<Texture2D>("ITD/Content/Tiles/Catacombs/BlueCatacombBrickTile_Glow");
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Item101;
            DustType = ModContent.DustType<BlueCatacombDust>();
            AddMapEntry(new Color(45, 45, 101));
            MinPick = 120;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileHelpers.DrawSlopedGlowMask(i, j, glowmask.Value, Color.White, Vector2.Zero);
            if (Main.rand.NextBool(16) && !Main.gameInactive)
                Rain.NewRainForced(new Point(i, j).ToWorldCoordinates() + Vector2.UnitY * 16f, new Vector2(1f, 16f));
        }
    }
}
