namespace ITD.Content.Tiles.LayersRework;

public class DepthrockBrickTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileBlendAll[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        MinPick = 45;
        HitSound = SoundID.Tink;
        DustType = DustID.Stone;

        AddMapEntry(new Color(82, 82, 90));
    }
}
