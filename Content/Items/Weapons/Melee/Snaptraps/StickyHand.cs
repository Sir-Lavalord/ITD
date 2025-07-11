using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using System.Collections.Generic;
using System.Linq;
using ITD.Utilities;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class StickyHand : ITDSnaptrapItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<StickyHandProjectile>(), 12f, 22, 27);
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 25, 0);
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