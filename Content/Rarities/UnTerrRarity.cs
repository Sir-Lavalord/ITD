using Daybreak.Common.Features.Rarities;
using ReLogic.Graphics;

namespace ITD.Content.Rarities;

public class UnTerrRarity : ModRarity, IRarityTextRenderer
{
    public override Color RarityColor => Color.Black;

    public void RenderText(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, RarityDrawContext drawContext, float maxWidth = -1, float spread = 2)
    {
        Color yellow = new(228, 202, 12);
        Color blue = new(83, 75, 164);
        Color darkBlue = new(17, 13, 53);
        RarityHelper.DrawBaseTooltipTextAndGlow(position, origin, scale, rotation, maxWidth, text, blue, darkBlue, yellow, null, new(1.2f, 1f));
    }
}
