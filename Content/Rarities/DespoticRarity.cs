using Daybreak.Common.Features.Rarities;
using ReLogic.Graphics;

namespace ITD.Content.Rarities;

public class DespoticRarity : ModRarity, IRarityTextRenderer
{
    public override Color RarityColor => Color.Black;
    public void RenderText(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, RarityDrawContext drawContext, float maxWidth = -1, float spread = 2)
    {
        Color blue = new(51, 191, 255);
        RarityHelper.DrawBaseTooltipTextAndGlow(position, origin, scale, rotation, maxWidth, text, blue, blue, Color.Black, null, new(1.2f, 1f));
    }
}
