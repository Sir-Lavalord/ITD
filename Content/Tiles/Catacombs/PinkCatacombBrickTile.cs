using ITD.Content.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Tiles.Catacombs
{
    public class PinkCatacombBrickTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Item101;
            DustType = DustID.Shadowflame;
            AddMapEntry(new Color(130, 53, 142));
            MinPick = 120;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
