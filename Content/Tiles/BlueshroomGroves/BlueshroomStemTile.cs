using ITD.Content.Dusts;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class BlueshroomStemTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileShine2[Type] = true;
            Main.tileNoSunLight[Type] = false;

            DustType = ModContent.DustType<BlueshroomStemDust>();
            HitSound = SoundID.Item50;

            AddMapEntry(new Color(210, 180, 140));
        }
    }
}