using ITD.Content.Dusts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class BlueshroomCapBlockTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Dig;
            DustType = ModContent.DustType<BlueshroomSporesDust>();
            AddMapEntry(Color.Cyan);
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
