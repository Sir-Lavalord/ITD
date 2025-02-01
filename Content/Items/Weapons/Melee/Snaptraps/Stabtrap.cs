using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;
using ITD.Utilities;
using ITD.Content.Tiles;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Stabtrap : ITDSnaptrapItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<StabtrapProjectile>(), 12f, 22, 30);
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 25);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<ElectrumBar>(), 8);//PLACEHOLDER
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
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