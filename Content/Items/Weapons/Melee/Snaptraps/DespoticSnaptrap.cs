using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Content.Rarities;
using ITD.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps;

public class DespoticSnaptrap : ITDSnaptrapItem
{
    public override void SetDefaults()
    {
        Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<DespoticSnaptrapProjectile>(), 12f, 8, 8900);
        Item.rare = ModContent.RarityType<DespoticRarity>();
        Item.value = Item.sellPrice(platinum: 1);
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