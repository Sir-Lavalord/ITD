using ITD.Content.Dusts;
using Terraria.Localization;
using Terraria.ObjectData;

namespace ITD.Content.Tiles;

internal class StarlitBars : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = true;
        Main.tileShine[Type] = 1100;
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        DustType = ModContent.DustType<StarlitDust>();

        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.MetalBar"));
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        Tile tile = Main.tile[i, j];
        r = 0.4f;
        g = 0.15f;
        b = 0.4f;
    }
}