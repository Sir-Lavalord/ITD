using ITD.Content.Dusts;

namespace ITD.Content.Tiles.Catacombs
{
    public class MossyGreenCatacombBrickTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            HitSound = SoundID.Item101;
            DustType = ModContent.DustType<GreenCatacombDust>();
            AddMapEntry(new Color(69, 120, 80));
            MinPick = 120;
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
