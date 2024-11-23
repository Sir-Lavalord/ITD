using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.DeepDesert
{
    public class LightPyracottaTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = DustID.Sandstorm;

            AddMapEntry(new Color(196, 162, 126));
        }
    }
}
