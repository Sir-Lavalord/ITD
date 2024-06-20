using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Walls
{
    public class PegmatiteWallUnsafe : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(79, 56, 62));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
