using ITD.Content.Dusts;

namespace ITD.Content.Tiles.DeepDesert;

public class LightPyracottaTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        MinPick = 55;
        HitSound = SoundID.Tink;
        DustType = ModContent.DustType<PyracottaDust>();

        AddMapEntry(new Color(196, 162, 126));
    }
}
