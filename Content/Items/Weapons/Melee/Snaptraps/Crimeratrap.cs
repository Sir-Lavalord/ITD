using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using System.Collections.Generic;
using System.Linq;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps;

public class Crimeratrap : ITDSnaptrapItem
{
    public override void SetDefaults()
    {
        Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<CrimeratrapProjectile>(), 12f, 22, 50);
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(0, 0, 25);
    }
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Vertebrae, 10);
        recipe.AddIngredient(ItemID.Chain, 12);
        recipe.AddTile(TileID.DemonAltar);
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