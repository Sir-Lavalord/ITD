using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using System.Collections.Generic;
using System.Linq;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps;

public class VocalZero : ITDSnaptrapItem
{
    public override void SetDefaults()
    {
        Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<VocalZeroProjectile>(), 16f, 16, 3200, SoundID.NPCDeath13);
        Item.rare = ItemRarityID.Purple;
        Item.value = Item.sellPrice(0, 0, 25);
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