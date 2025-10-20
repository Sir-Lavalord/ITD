using Terraria.Achievements;
using Terraria.GameContent.Achievements;

namespace ITD.Content.Achievements;
public sealed class PARRY : ModAchievement
{
    public CustomFlagCondition ParryCondition { get; private set; }
    public override void SetStaticDefaults()
    {
        Achievement.SetCategory(AchievementCategory.Challenger);
        ParryCondition = AddCondition();
    }
}
