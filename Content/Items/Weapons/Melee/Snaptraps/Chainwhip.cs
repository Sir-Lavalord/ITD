﻿using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using ITD.Utilities;
using Terraria.DataStructures;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Content.Projectiles;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Chainwhip : ModItem
    {
        public override void SetDefaults()
        {
                Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<ChainwhipSnaptrapProjectile>(), 12f, 22, 50);
                Item.rare = ItemRarityID.LightRed;
                Item.value = Item.sellPrice(0, 0, 25);
        }
        public override bool CanUseItem(Player player) => player.GetSnaptrapPlayer().CanUseSnaptrap;
        public override bool AltFunctionUse(Player player) => true;
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
                ITDSnaptrap snaptrap = player.GetSnaptrapPlayer().GetActiveSnaptrap();
                if (snaptrap != null && snaptrap.IsStickingToTarget)
                {
                    type = ModContent.ProjectileType<ChainwhipWhip>();
                    return;
                }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float pulseAmount = Main.mouseTextColor / 255f;
            Color textColor = Color.LightPink * pulseAmount;
            var line = tooltips.First(x => x.Name == "Tooltip1");
            string coloredText = string.Format(line.Text, textColor.Hex3());
            line.Text = coloredText;
        }
    }
}