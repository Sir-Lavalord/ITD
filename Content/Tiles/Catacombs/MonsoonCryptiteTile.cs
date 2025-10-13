using ITD.Utilities;

namespace ITD.Content.Tiles.Catacombs;

public class MonsoonCryptiteTile : ModTile
{
    private Asset<Texture2D> glowmask;
    public override void SetStaticDefaults()
    {
        glowmask = Mod.Assets.Request<Texture2D>("Content/Tiles/Catacombs/MonsoonCryptiteTile_Glow");
        Main.tileSolid[Type] = true;
        HitSound = SoundID.Item85;
        DustType = DustID.WaterCandle;
        AddMapEntry(new Color(44, 50, 84));
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
