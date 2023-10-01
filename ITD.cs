using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace ITD
{
	public class ITD : ModSystem
	{
        public override void AddRecipeGroups()
        {
            RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Iron Ore", new int[]
            {
        ItemID.IronOre,
        ItemID.LeadOre
            });
            RecipeGroup.RegisterGroup("IronOre", group);
        }
    }
}