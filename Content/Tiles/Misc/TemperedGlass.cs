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
            MinPick = 30;

            HitSound = SoundID.Item48;
            DustType = DustID.Glass;

            AddMapEntry(Color.LightSkyBlue);
        }
    }
}