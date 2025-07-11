namespace ITD.Content.Walls.DeepDesert
{
    public class PegmatiteWallUnsafe : ModWall
    {
        public override void SetStaticDefaults()
        {
            // what a strangely specific set. but useful (since we want to avoid these spawns in the deep desert)
            WallID.Sets.AllowsUndergroundDesertEnemiesToSpawn[Type] = false;
            AddMapEntry(new Color(79, 56, 62));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
