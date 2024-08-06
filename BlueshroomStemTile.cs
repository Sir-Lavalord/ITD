using ITD.Content.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

            HitSound = SoundID.Item50;

            AddMapEntry(new Color(210, 180, 140));
        }
    }
}