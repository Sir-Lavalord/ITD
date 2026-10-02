using ITD.Content.NPCs.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace ITD.Content.BossBars
{
    public class WispBossBar : ModBossBar
    {
        private int bossHeadIndex = -1;
        private int lastNpcIndex = -1;

        public override string Texture => "ITD/Content/BossBars/WispBossBar";
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
        {
            return null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
        {
            MotherWisp wisp = npc.ModNPC as MotherWisp;
            if (wisp == null) return false;

            if (npc.dontTakeDamage && wisp.MainAttack != (float)MotherWisp.BaseAttack.Split && wisp.AI_State != (float)MotherWisp.ActionState.Spawning)
                return false;

            if (npc.whoAmI != lastNpcIndex)
            {
                lastNpcIndex = npc.whoAmI;
            }

            Vector2 barCenter = drawParams.BarCenter;
            Texture2D barTex = drawParams.BarTexture;
            if (barTex == null) return false;

            float uiAlpha = 1f;
            Color frameColor = Color.White * uiAlpha;

            bool isSplitMode = wisp.MainAttack == (float)MotherWisp.BaseAttack.Split && npc.dontTakeDamage && npc.Opacity <= 0f;

            Vector2 shakenOffset = Vector2.Zero;
            if (isSplitMode)
            {
                shakenOffset = Main.rand.NextVector2Circular(3f, 3f);
            }

            int frameWidth = barTex.Width;
            int frameHeight = barTex.Height / 6;
            int baseSourceHeight = frameHeight + 20;
            float baseDrawOffsetY = 0f;
            int tipSourceExpandUp = 12;
            float tipDrawOffsetY = 0f;
            int hpOffsetX = 32;
            int frontRightPadding = 60;
            int hpMaxFillWidthFront = frameWidth - hpOffsetX - frontRightPadding;
            int backRightPadding = 60;
            int hpMaxFillWidthBack = frameWidth - hpOffsetX - backRightPadding;
            Rectangle candleBaseSource = new Rectangle(0, 0, 100, baseSourceHeight);
            Rectangle normalBodySource = new Rectangle(0, frameHeight * 1, frameWidth, frameHeight);
            Rectangle normalTipSource = new Rectangle(0, frameHeight * 2 - tipSourceExpandUp, frameWidth, frameHeight + tipSourceExpandUp);
            Rectangle splitBodySource = new Rectangle(0, frameHeight * 4, frameWidth, frameHeight);
            Rectangle splitTipSource = new Rectangle(0, frameHeight * 5 - tipSourceExpandUp, frameWidth, frameHeight + tipSourceExpandUp);

            Rectangle currentBody = isSplitMode ? splitBodySource : normalBodySource;
            Rectangle currentTipLayer = isSplitMode ? splitTipSource : normalTipSource;

            Vector2 frameTopLeft = barCenter - new Vector2(frameWidth / 2f, frameHeight / 2f);

            Vector2 shakenTopLeft = frameTopLeft + shakenOffset;

            float hpPercent = 0f;
            string hpText;

            if (isSplitMode)
            {
                int remainingWisps = wisp.maxWispCount - (int)wisp.GrandWispsLost;
                if (wisp.maxWispCount > 0)
                {
                    hpPercent = MathHelper.Clamp((float)remainingWisps / (float)wisp.maxWispCount, 0f, 1f);
                }
                hpText = $"{remainingWisps}/{wisp.maxWispCount}";
            }
            else
            {
                if (npc.lifeMax > 0)
                {
                    hpPercent = MathHelper.Clamp((float)npc.life / (float)npc.lifeMax, 0f, 1f);
                }
                hpText = $"{npc.life}/{npc.lifeMax}";
            }

            int frontFillWidth = (int)(hpMaxFillWidthFront * hpPercent);
            int backFillWidth = (int)(hpMaxFillWidthBack * hpPercent);

            Color candleTint = isSplitMode ? new Color(131, 255, 236) * uiAlpha : frameColor;

            if (backFillWidth > 0)
            {
                int shiftAmount = hpMaxFillWidthBack - backFillWidth;

                Rectangle backLayerSource = currentTipLayer;
                backLayerSource.X = hpOffsetX + shiftAmount;
                backLayerSource.Width = frameWidth - backLayerSource.X;

                Vector2 backLayerPos = shakenTopLeft + new Vector2(hpOffsetX, -tipSourceExpandUp - tipDrawOffsetY);
                spriteBatch.Draw(barTex, backLayerPos, backLayerSource, candleTint);
            }
            if (frontFillWidth > 0)
            {
                int cropWidth = hpOffsetX + frontFillWidth;
                Rectangle frontLayerSource = currentBody;
                frontLayerSource.Width = cropWidth;

                spriteBatch.Draw(barTex, shakenTopLeft, frontLayerSource, candleTint);
            }

            Vector2 candlePos = frameTopLeft + new Vector2(0f, -baseDrawOffsetY);

            if (isSplitMode)
            {
                float time = Main.GlobalTimeWrappedHourly;
                float timer = (float)Main.time / 240f + time * 0.04f;

                time %= 4f;
                time /= 2f;

                if (time >= 1f) time = 2f - time;
                time = time * 0.5f + 0.75f;
                Color outlineColor = new Color(131, 255, 236, 150) * uiAlpha;

                for (float i = 0f; i < 1f; i += 0.2f)
                {
                    float radians = (i + timer) * MathHelper.TwoPi;
                    Vector2 outlineOffset = new Vector2(0f, 3f).RotatedBy(radians) * time;
                    spriteBatch.Draw(barTex, candlePos + outlineOffset, candleBaseSource, outlineColor);
                }
            }
            spriteBatch.Draw(barTex, candlePos, candleBaseSource, frameColor);

            Vector2 textPos = frameTopLeft + new Vector2(frameWidth / 2f + 20f, frameHeight / 2f + 2f);
            Utils.DrawBorderString(spriteBatch, hpText, textPos, frameColor, 1f, 0.5f, 0.5f);

            return false;
        }

        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
        {
            NPC npc = Main.npc[info.npcIndexToAimAt];
            if (npc.townNPC || !npc.active || npc.type != ModContent.NPCType<MotherWisp>())
                return false;

            life = npc.life;
            lifeMax = npc.lifeMax;

            bossHeadIndex = npc.GetBossHeadTextureIndex();
            return true;
        }
    }
}