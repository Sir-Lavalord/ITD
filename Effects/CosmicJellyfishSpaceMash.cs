﻿using ITD.Content.NPCs.Bosses;
using ITD.Players;
using Terraria.DataStructures;

namespace ITD.Effects
{
    public class CosmicJellyfishSpaceMash : PlayerDrawLayer
    {
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.active
        && NPC.AnyNPCs(ModContent.NPCType<CosmicJellyfish>())
        && !drawInfo.drawPlayer.dead
        && !drawInfo.drawPlayer.ghost
        && drawInfo.shadow == 0
        && drawInfo.drawPlayer.GetModPlayer<ITDPlayer>().CosJellSuffocated;

        public override Position GetDefaultPosition()
        {
            return PlayerDrawLayers.AfterLastVanillaLayer;
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.shadow != 0f)
            {
                return;
            }

            Player drawPlayer = drawInfo.drawPlayer;
            ITDPlayer modPlayer = drawPlayer.GetModPlayer<ITDPlayer>();

            if (++modPlayer.frameCounter >= 30)
            {
                modPlayer.frameCounter = 0;
                if (++modPlayer.frameEffect >= 2)
                    modPlayer.frameEffect = 0;
            }

            Texture2D texture = ModContent.Request<Texture2D>("ITD/Effects/CosmicJellyfishSpaceMash", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int frameSize = texture.Height / 2;
            int drawX = (int)(drawPlayer.MountedCenter.X - Main.screenPosition.X);
            int drawY = (int)((drawPlayer.MountedCenter.Y - Main.screenPosition.Y - 16 * drawPlayer.gravDir) + 60);
            DrawData data = new(texture, new Vector2(drawX, drawY), new Rectangle(0, frameSize * modPlayer.frameEffect,
                texture.Width, frameSize), Color.White, drawPlayer.gravDir < 0 ? MathHelper.Pi : 0,
                new Vector2(texture.Width / 2f, frameSize / 2f), 1f, drawPlayer.direction < 0 ? SpriteEffects.None
                : SpriteEffects.None, 0);
            drawInfo.DrawDataCache.Add(data);

        }
    }
}