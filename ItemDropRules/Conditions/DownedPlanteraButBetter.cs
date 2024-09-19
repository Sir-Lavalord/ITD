using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Terraria;

namespace ITD.ItemDropRules.Conditions
{
    public class DownedPlanteraButBetter : IItemDropRuleCondition
    {
		protected readonly string description;
		
        public bool CanDrop(DropAttemptInfo info)
        {
            if (info.IsInSimulation)
                return false;

            return NPC.downedPlantBoss;
        }

        public bool CanShowItemDropInUI()
        {
            return NPC.downedPlantBoss;
        }
		
		public string GetConditionDescription()
        {
            return description;
        }
    }
}
