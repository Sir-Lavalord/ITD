using ITD.Content.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Tiles.Catacombs
{
    public class BlueCatacombBrickTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Item101;
            DustType = ModContent.DustType<BlueCatacombDust>();
            AddMapEntry(new Color(45, 45, 101));
            MinPick = 120;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
