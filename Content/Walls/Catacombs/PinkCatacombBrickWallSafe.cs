using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Walls.Catacombs
{
    public class PinkCatacombBrickWallSafe : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(42, 32, 42));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
