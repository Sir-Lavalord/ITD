using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Diagnostics;

namespace ITD.Content.NPCs
{
    public class EmberlionPiercer : ModNPC
    {
        private readonly Asset<Texture2D> glowmask = ModContent.Request<Texture2D>("ITD/Content/NPCs/EmberlionPiercer_Glow");
        private enum ActionState
        {
            Asleep,
            Noticed,
            Dashing,
            Chasing
        }
        private ActionState AI_State;
        private float glowmaskOpacity;
        public int dashingTimer;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4; // make sure to set this for your modnpcs.

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }
        public override void SetDefaults()
        {
            glowmaskOpacity = 0f;
            AI_State = (float)ActionState.Asleep;
            NPC.width = 72;
            NPC.height = 36;
            NPC.damage = 60;
            NPC.defense = 20;
            NPC.lifeMax = 150;
            NPC.HitSound = SoundID.NPCHit31;
            NPC.DeathSound = SoundID.NPCDeath34;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.aiStyle = NPCAIStyleID.Fighter;
            AIType = NPCID.GiantWalkingAntlion;
        }
        public override bool PreAI()
        {
            Player player = Main.player[NPC.target];
            Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            switch (AI_State)
            {
                case ActionState.Asleep:
                    NPC.TargetClosest(false);
                    if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height) && toPlayer.Length() < 20f * 60f)
                    {
                        AI_State = ActionState.Noticed;
                    }
                    return false;
                case ActionState.Noticed:
                    if (glowmaskOpacity < 1f)
                    {
                        glowmaskOpacity += 0.01f;
                    }
                    else
                    {
                        if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height) && toPlayer.Length() < 10f * 60f)
                        {
                            AI_State = ActionState.Dashing;
                        }
                    }
                    return false;
                case ActionState.Dashing:
                    NPC.TargetClosest(true);
                    NPC.velocity += toPlayer * 0.8f;
                    NPC.velocity.X = Math.Clamp(NPC.velocity.X, -8f, 8f);
                    dashingTimer++;
                    Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Torch);
                    Dust.NewDust(NPC.Center, NPC.width, NPC.height, DustID.Flare);
                    if (dashingTimer < 3)
                    {
                        Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, 62);
                        Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, 61);
                        Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, 63);
                    }
                    if (dashingTimer > 30)
                    {
                        AI_State = ActionState.Chasing;
                    }
                    return false;
                default: return true;
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            spriteBatch.Draw(glowmask.Value, NPC.position - screenPos + new Vector2(0f, 2f), NPC.frame, Color.White * glowmaskOpacity, 0f, Vector2.Zero, 1f, NPC.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, default);
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.spriteDirection = NPC.direction;
            if (NPC.velocity.Y == 0f && AI_State == ActionState.Chasing)
            {
                NPC.frameCounter += 1f;
                if (NPC.frameCounter > 5f)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > frameHeight * Main.npcFrameCount[Type]-1)
                    {
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
        }
    }
}
