﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ITD.Content.Dusts;

namespace ITD.Content.Tiles.DeepDesert
{
    public class LightPyracottaTiles : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = ModContent.DustType<PyracottaDust>();

            AddMapEntry(new Color(196, 162, 126));
        }
    }
    public class DarkPyracottaTiles : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;;

            MinPick = 55;
            HitSound = SoundID.Tink;
            DustType = ModContent.DustType<PyracottaDust>();

            AddMapEntry(new Color(191, 88, 65));
        }
    }
}
