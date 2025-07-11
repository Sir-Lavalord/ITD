namespace ITD.Content.Walls.Catacombs
{
    public class CryptiteWallSafe : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(44, 27, 42));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
