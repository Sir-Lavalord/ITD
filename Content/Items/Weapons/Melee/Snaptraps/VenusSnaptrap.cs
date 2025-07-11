using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using System.Collections.Generic;
using System.Linq;
using ITD.Utilities;
namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class VenusSnaptrap : ITDSnaptrapItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<VenusSnaptrapProjectile>(), 12f, 22, 25);
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 5, 0);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.JungleSpores, 10);
            recipe.AddIngredient(ItemID.Vine,3);
            recipe.AddIngredient(ItemID.Stinger, 6);
            recipe.AddTile(TileID.Anvils);
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