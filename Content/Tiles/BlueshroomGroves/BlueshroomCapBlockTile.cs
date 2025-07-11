using ITD.Content.Dusts;

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
