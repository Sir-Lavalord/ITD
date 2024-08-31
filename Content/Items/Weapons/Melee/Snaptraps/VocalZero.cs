using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class VocalZero : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 16;
            Item.knockBack = 0f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 3200;
            Item.shoot = ModContent.ProjectileType<VocalZeroProjectile>();
            Item.shootSpeed = 16f;
            Item.UseSound = SoundID.NPCDeath13;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.sellPrice(0, 0, 25);
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            float pulseAmount = Main.mouseTextColor / 255f;
            Color textColor = Color.LightPink * pulseAmount;
            var line = tooltips.First(x => x.Name == "Tooltip0");
            string coloredText = string.Format(line.Text, textColor.Hex3());
            line.Text = coloredText;
        }
    }
}