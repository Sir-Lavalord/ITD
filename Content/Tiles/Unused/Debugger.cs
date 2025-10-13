namespace ITD.Content.Tiles.Unused;

public class Debugger : ModTile
{
    public override void SetStaticDefaults()
    {
        var name = CreateMapEntryName();
        AddMapEntry(Color.Red, name);
    }
}
