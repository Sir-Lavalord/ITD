using ITD.Content.Dusts;

namespace ITD.Content.Tiles.Catacombs;

public class GreenCatacombBrickTile : ModTile
{
    private Asset<Texture2D> glowmask;
    public override void SetStaticDefaults()
    {
        glowmask = Mod.Assets.Request<Texture2D>("Content/Tiles/Catacombs/GreenCatacombBrickTile_Glow");
        Main.tileSolid[Type] = true;
        HitSound = SoundID.Item101;
        DustType = ModContent.DustType<GreenCatacombDust>();
        AddMapEntry(new Color(64, 112, 69));
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
