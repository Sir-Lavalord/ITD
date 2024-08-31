using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Snaptrap : ModItem
    {
        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 20;
            Item.useTime = 22;
            Item.knockBack = 0f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 50;
            Item.shoot = ModContent.ProjectileType<SnaptrapProjectile>();
            Item.shootSpeed = 12f;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 0, 25);
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }

        public override bool CanUseItem(Player player)
        {
            return (player.ownedProjectileCounts[Item.shoot] <= 0);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(); // changethis
            recipe.AddIngredient(ItemID.IronBar, 6);
            recipe.AddIngredient(ItemID.Chain, 16);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
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