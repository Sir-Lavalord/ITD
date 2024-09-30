using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ITD.Content.Items.Other
{
    public class BottomlessSandBucket : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.width = 20;
			Item.height = 20;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.master = true;

			Item.tileBoost += 2;
			
			Item.ammo = AmmoID.Sand;
			Item.notAmmo = true;
			
			Item.createTile = TileID.Sand;
        }
		
		public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
			{
				if (line.Mod == "Terraria" && line.Name == "Placeable")
				{
					line.Text = "";
                }
            }
		}
    }
}