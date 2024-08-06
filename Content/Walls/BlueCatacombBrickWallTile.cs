using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Walls
{
    public class BlueCatacombBrickWallTile : ModWall
    {
        public override void SetStaticDefaults()
        {
            AddMapEntry(new Color(17, 31, 42));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
