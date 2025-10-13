using ITD.Utilities;

namespace ITD.Content.NPCs.BlueshroomGroves.Critters;

public class SmallSnowpoff : GenericSnowpoff
{
    public override int NormalHeight => 20;
    public override int HeightAsBall => 32;
    public override void SetDefaults()
    {
        base.SetDefaults();
        NPC.defense = 5;
        NPC.lifeMax = 10;
        NPC.catchItem = ModContent.ItemType<SmallSnowpoffItem>();
    }
    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
            return 0.25f;
        return 0f;
    }
}
public class SmallSnowpoffItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToCapturedCritter(ModContent.NPCType<SmallSnowpoff>());
        Item.value = Item.buyPrice(silver: 15);
        Item.rare = ItemRarityID.Blue;
    }
}
