using ITD.Content.NPCs.Bosses;

namespace ITD.Content.Achievements;
public sealed class CosmicConundrum : ModAchievement
{
    public override void SetStaticDefaults() => AddNPCKilledCondition(ModContent.NPCType<CosmicJellyfish>());
}
