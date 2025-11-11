using Terraria.GameContent.Drawing;
using Terraria.Localization;
using Terraria.ObjectData;

namespace ITD.Content.Tiles;

public abstract class ITDCandle : ModTile
{
    public virtual Asset<Texture2D> FlameTexture => null;
    public virtual int ItemType => ItemID.Candle;
    public virtual Color MapColor => Color.White;
    public virtual Vector3 GetLightColor(int i, int j)
    {
        return Color.White.ToVector3();
    }
    public sealed override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleOnTable1x1);
        TileObjectData.addTile(Type);

        AddMapEntry(MapColor, Language.GetText("MapObject.Candle"));

        SetStaticDefaultsSafe();
    }
    public virtual void SetStaticDefaultsSafe()
    {

    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        if (Main.tile[i, j].TileFrameX < 18)
        {
            Vector3 lightColor = GetLightColor(i, j);
            r = lightColor.X;
            g = lightColor.Y;
            b = lightColor.Z;
            return;
        }
        r = g = b = 0f;
    }
    public override void MouseOver(int i, int j)
    {
        Player p = Main.LocalPlayer;
        p.noThrow = 2;
        p.cursorItemIconEnabled = true;
        p.cursorItemIconID = ItemType;
    }
    public override void HitWire(int i, int j)
    {
        TileHelpers.CommonWiringLight(Type, i, j);
    }
    public override bool RightClick(int i, int j)
    {
        WorldGen.KillTile(i, j);
        NetMessage.SendData(MessageID.TileManipulation, number2: i, number3: j);
        return true;
    }
    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (FlameTexture is null)
            return;

        Tile tile = Framing.GetTileSafely(i, j);

        if (!TileDrawing.IsVisible(tile))
        {
            return;
        }

        TileObjectData tod = TileObjectData.GetTileData(tile);

        if (tile.TileFrameX < 18)
        {
            ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (uint)i);

            Vector2 zero = TileHelpers.CommonTileOffset;

            for (int c = 0; c < 7; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -10, 11) * 0.15f;
                float shakeY = Utils.RandomInt(ref randSeed, -10, 1) * 0.35f;

                Vector2 position = new Vector2(i * 16f - (int)Main.screenPosition.X + shakeX, j * 16 - (int)Main.screenPosition.Y + shakeY) + zero;
                spriteBatch.Draw(FlameTexture.Value, position, new Rectangle(tile.TileFrameX, tile.TileFrameY, tod.CoordinateWidth, tod.CoordinateHeights[0]), Color.White, 0f, default, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
