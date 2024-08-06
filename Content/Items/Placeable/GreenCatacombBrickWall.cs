﻿using ITD.Content.Walls;
using Terraria.ModLoader;

namespace ITD.Content.Items.Placeable
{
    public class GreenCatacombBrickWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableWall(ModContent.WallType<GreenCatacombBrickWallSafe>());
        }
    }
}