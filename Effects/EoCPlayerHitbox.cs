using ITD.Content.Items.Accessories.Master;
using ITD.Content.Items.Other;
using ITD.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ITD.Effects
{
    public class EoCPlayerHitbox : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.active
        && !drawInfo.drawPlayer.dead
        && !drawInfo.drawPlayer.ghost
        && drawInfo.shadow == 0
        && drawInfo.drawPlayer.GetModPlayer<InsightedPlayer>().CorporateInsight;

        public override Position GetDefaultPosition()
        {
            return PlayerDrawLayers.AfterLastVanillaLayer;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            Player drawPlayer = drawInfo.drawPlayer;
            ITDPlayer modPlayer = drawPlayer.GetModPlayer<ITDPlayer>();
            Rectangle hitbox = drawPlayer.getRect();
            hitbox.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);
            hitbox = Main.ReverseGravitySupport(hitbox);
            DrawData data = new(TextureAssets.MagicPixel.Value, hitbox, Color.LimeGreen * 0.3f);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}