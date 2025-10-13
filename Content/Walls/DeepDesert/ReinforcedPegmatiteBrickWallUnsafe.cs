namespace ITD.Content.Walls.DeepDesert;

public class ReinforcedPegmatiteBrickWallUnsafe : ModWall
{
    public override void SetStaticDefaults()
    {
        WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn[Type] = false;
        AddMapEntry(new Color(81, 53, 54));
        DustType = DustID.Sandstorm;
    }
    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}
