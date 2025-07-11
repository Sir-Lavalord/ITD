using ITD.Utilities;

namespace ITD.Content.NPCs.BlueshroomGroves.Critters
{
    public class LargeSnowpoff : GenericSnowpoff
    {
        public override int NormalHeight => 28;
        public override int HeightAsBall => 42;
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.defense = 10;
            NPC.lifeMax = 30;
            NPC.catchItem = ModContent.ItemType<LargeSnowpoffItem>();
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
                return 0.2f;
            return 0f;
        }
    }
    public class LargeSnowpoffItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<LargeSnowpoff>());
            Item.value = Item.buyPrice(silver: 15);
            Item.rare = ItemRarityID.Blue;
        }
    }
}
