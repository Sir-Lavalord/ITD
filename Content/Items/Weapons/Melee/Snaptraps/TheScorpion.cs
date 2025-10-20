using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps;

public class TheScorpion : ITDSnaptrapItem
{
    public override void SetDefaults()
    {
        Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<TheScorpionProjectile>(), 12f, 20, 75);
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(0, 0, 25);
    }
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.FossilOre, 16);
        recipe.AddIngredient(ItemID.Chain, 16);
        recipe.AddIngredient(ItemID.AncientBattleArmorMaterial, 1);
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