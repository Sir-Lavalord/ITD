namespace ITD.Content.Tiles.LayersRework;

public class ShaleBrickTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileBlendAll[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        MinPick = 40;
        HitSound = SoundID.Tink;
        DustType = DustID.Stone;

        AddMapEntry(new Color(92, 92, 92));
    }
}
