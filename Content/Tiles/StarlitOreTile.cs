using ITD.Content.Dusts;

namespace ITD.Content.Tiles
{
    public class StarlitOreTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
			Main.tileLighted[Type] = true;

            HitSound = SoundID.Tink;
            DustType = ModContent.DustType<StarlitDust>();

            AddMapEntry(new Color(15, 13, 59), CreateMapEntryName());
        }
		
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Tile tile = Main.tile[i, j];
			r = 0.4f;
			g = 0.15f;
			b = 0.4f;
		}
    }
}