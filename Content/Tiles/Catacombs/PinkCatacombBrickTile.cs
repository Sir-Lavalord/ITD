using ITD.Content.Dusts;
using ITD.Utilities;

namespace ITD.Content.Tiles.Catacombs
{
    public class PinkCatacombBrickTile : ModTile
    {
        private Asset<Texture2D> glowmask;
        public override void SetStaticDefaults()
        {
            glowmask = ModContent.Request<Texture2D>("ITD/Content/Tiles/Catacombs/PinkCatacombBrickTile_Glow");
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Item101;
            DustType = ModContent.DustType<PinkCatacombDust>();
            AddMapEntry(new Color(130, 53, 142));
            MinPick = 120;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            TileHelpers.DrawSlopedGlowMask(i, j, glowmask.Value, Color.White, Vector2.Zero);
        }
    }
}
