﻿using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using ITD.Utilities;
using Terraria.DataStructures;
using Terraria.Utilities;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public abstract class ITDSnaptrapItem : ModItem
    {
        public override void SetDefaults()
        {
        }
        public override bool CanUseItem(Player player) => player.GetSnaptrapPlayer().CanUseSnaptrap;
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) => player.GetSnaptrapPlayer().ShootSnaptrap();
        //not all of them (Vocal Zero) use the same tooltip line
        /*        public override void ModifyTooltips(List<TooltipLine> tooltips)
                {
                    float pulseAmount = Main.mouseTextColor / 255f;
                    Color textColor = Color.LightPink * pulseAmount;
                    var line = tooltips.First(x => x.Name == "Tooltip1");
                    string coloredText = string.Format(line.Text, textColor.Hex3());
                    line.Text = coloredText;
                }*/
        public override int ChoosePrefix(UnifiedRandom rand)
        {
            return PrefixID.Demonic;
        }
    }
}