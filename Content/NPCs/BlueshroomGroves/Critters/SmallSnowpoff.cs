using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.BlueshroomGroves.Critters
{
    public class SmallSnowpoff : GenericSnowpoff
    {
        public override int NormalHeight => 20;
        public override int HeightAsBall => 32;
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.defense = 50;
            NPC.lifeMax = 10;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
                return 0.1f;
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
}
