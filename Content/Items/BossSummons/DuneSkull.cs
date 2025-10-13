using ITD.Content.NPCs.Bosses;
using Terraria.GameContent.Events;

namespace ITD.Content.Items.BossSummons;

public class DuneSkull : BossSummoner
{
    public override int NPCType => ModContent.NPCType<Sandberus>();
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 20;
        Item.value = 100;
        Item.rare = ItemRarityID.Blue;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = true;
    }

    public override bool CanUseItem(Player player)
    {
        return !NPC.AnyNPCs(NPCType) && player.ZoneDesert && Sandstorm.Happening;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.FossilOre, 2)
            .AddIngredient(ItemID.DesertFossil, 4)
            .AddIngredient(ItemID.SandBlock, 100)
            .Register();
    }
}
