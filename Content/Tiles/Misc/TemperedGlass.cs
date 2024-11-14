using ITD.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Buffs.Debuffs;
using Microsoft.Xna.Framework.Graphics;

namespace ITD.Content.Tiles.Misc
{
    public class TemperedGlass : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;

            MinPick = 30;
            MineResist = 0.001f;

            HitSound = SoundID.Shatter;
            DustType = DustID.Glass;

            AddMapEntry(Color.LightSkyBlue);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            
        }
    }
}