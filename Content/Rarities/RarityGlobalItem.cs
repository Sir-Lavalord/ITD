using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Rarities
{
    public class CustomRarityGlobalItem : GlobalItem // it also came from the infernum
    {
        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            // If the item is of the rarity, and the line is the item name.
            if (line.Mod == "Terraria" && line.Name == "ItemName")
            {
                if (item.rare == ModContent.RarityType<DespoticRarity>())
                {
                    // Draw the custom tooltip line.
                    DespoticRarity.DrawCustomTooltipLine(line);
                    return false;
                }
            }
            return true;
        }
    }
}
