using ITD.Content.Buffs.Debuffs;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Bosses;

public class GrandWisp : ModNPC
{
    public ParticleEmitter emitter;
    public override string Texture => "ITD/Content/NPCs/Bosses/GrandWisp";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 6;
    }

    public override void SetDefaults()
    {
        NPC.width = 100;
        NPC.height = 100;
        NPC.damage = 30;
        NPC.defense = 0;
        NPC.lifeMax = 500;
        NPC.HitSound = SoundID.NPCHit37;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;

        NPC.knockBackResist = 0.6f;

        NPC.aiStyle = -1;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
    }

    public int OwnerIndex => (int)NPC.ai[0];

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.2f * balance * bossAdjustment);
        NPC.damage = (int)(NPC.damage * 0.5f);
    }

    public override void OnSpawn(IEntitySource source)
    {
        NPC.scale = 1.5f;
        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null)
        {
            NPC.timeLeft = 0;
            NPC.active = false;
            return;
        }
        if (emitter != null)
            emitter.keptAlive = true;
    }

    public override bool CheckDead()
    {
        if (NPC.ai[3] == 0)
        {
            NPC.ai[3] = 1;
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            NPC.netUpdate = true;
            return false;
        }
        return true;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
    {
        return NPC.ai[1] == 0 && NPC.ai[3] == 0 && NPC.localAI[2] > 60f;
    }

    public override void AI()
    {
        if (NPC.localAI[2] < 100f)
        {
            NPC.localAI[2]++;
        }

        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null)
        {
            NPC.timeLeft = 0;
            NPC.active = false;
            return;
        }

        if (emitter != null)
            emitter.keptAlive = true;

        if (Main.rand.NextBool(4))
        {
            emitter?.Emit(NPC.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2) * NPC.scale, -NPC.velocity, 0f, 90);
        }

        if (NPC.ai[1] == 0)
        {
            if (NPC.localAI[0]++ >= 30)
            {
                NPC.TargetClosest(false);

                NPC candle = MiscHelpers.NPCExists((int)Mom.ai[0], ModContent.NPCType<WispCandle>());
                Vector2 anchorPos = candle != null && candle.active ? candle.Center : Mom.Center;

                float maxCandleDist = 350f;
                float pullStrength = 0.2f;
                float wispPushRadius = 100f;
                float wispPushForce = 0.4f;

                if (Main.rand.NextBool(40))
                {
                    Vector2 randomVel = Main.rand.NextVector2Circular(5f, 5f);
                    NPC.velocity += randomVel;
                }

                float distToAnchor = Vector2.Distance(NPC.Center, anchorPos);
                if (distToAnchor > maxCandleDist)
                {
                    Vector2 pullDir = Vector2.Normalize(anchorPos - NPC.Center);
                    NPC.velocity += pullDir * pullStrength * (distToAnchor / maxCandleDist);
                }

                Vector2 pushForceVec = Vector2.Zero;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];

                    if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type && (int)other.ai[0] == OwnerIndex && other.ai[1] == 0)
                    {
                        float dist = Vector2.Distance(NPC.Center, other.Center);

                        if (dist < wispPushRadius && dist > 0.1f)
                        {
                            Vector2 pushAway = Vector2.Normalize(NPC.Center - other.Center);
                            float pushMultiplier = (wispPushRadius - dist) / wispPushRadius;
                            pushForceVec += pushAway * (wispPushForce * pushMultiplier);
                        }
                    }
                }
                NPC.velocity += pushForceVec;

                if (NPC.velocity.LengthSquared() > 18f * 18f)
                {
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * 18f;
                }

                NPC.velocity *= 0.96f;
            }
        }
        else
        {
            NPC.dontTakeDamage = true;
            NPC.damage = 0;

            if (NPC.ai[2] == 0)
            {
                NPC.localAI[0] = NPC.Center.X;
                NPC.localAI[1] = NPC.Center.Y;
            }

            NPC.ai[2]++;
            float duration = 90f;
            float progress = Math.Clamp(NPC.ai[2] / duration, 0f, 1f);
            float ease = progress * progress;

            Vector2 startPos = new Vector2(NPC.localAI[0], NPC.localAI[1]);
            NPC.Center = Vector2.Lerp(startPos, Mom.Center, ease);

            if (progress >= 1f || NPC.Distance(Mom.Center) < 20f)
            {
                NPC.active = false;
                NPC.netUpdate = true;
            }
        }

        float dieTime = 120;
        if (NPC.ai[3] > 0)
        {
            NPC.ai[3]++;
            if (NPC.ai[3] > dieTime)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                for (int i = 0; i < 20; i++)
                {
                    emitter?.Emit(NPC.Center, Main.rand.NextVector2Circular(10f, 10f), 0f, 60);
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.StrikeInstantKill();
                }
            }
        }
    }

    public override void OnKill()
    {
        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null) return;
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Mom.localAI[2] += 1f;
            Mom.netUpdate = true;
        }
    }

    int faceFrameCurrent = 0;
    int faceFrameCounter = 0;

    public void AnimateFace(int frameStart, int frameEnd, int frameSpeed, bool doLoop = true)
    {
        if (frameStart != -1 && (faceFrameCurrent < frameStart || faceFrameCurrent > frameEnd))
        {
            faceFrameCurrent = frameStart;
            faceFrameCounter = 0;
        }

        if (++faceFrameCounter >= frameSpeed)
        {
            faceFrameCounter = 0;
            faceFrameCurrent++;

            if (faceFrameCurrent > frameEnd)
            {
                if (doLoop)
                    faceFrameCurrent = frameStart == -1 ? 0 : frameStart;
                else
                    faceFrameCurrent = frameEnd;
            }
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (++NPC.frameCounter >= 10)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y = (NPC.frame.Y + frameHeight) % (6 * frameHeight);
        }

        if (NPC.ai[3] > 0)
        {
            AnimateFace(0, 2, 10, false);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D face = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/GrandWisp_Face").Value;

        Rectangle frameBody = texture.Frame(1, 6, 0, NPC.frame.Y / (texture.Height / 6));
        Rectangle frameFace = face.Frame(1, 3, 0, faceFrameCurrent);

        Texture2D glowOrb = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle glowOrbFrame = glowOrb.Frame(1, 1, 0, 0);

        Vector2 drawOffset = Vector2.Zero;
        if (NPC.ai[3] > 0)
        {
            drawOffset = Main.rand.NextVector2Circular(6f, 6f);
        }

        void DrawAtNPC(Texture2D tex, Rectangle rect, float scale)
        {
            sb.Draw(tex, NPC.Center + drawOffset + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, rect, Color.White * NPC.Opacity, NPC.rotation,
                rect.Size() / 2f, scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => Main.EntitySpriteDraw(glowOrb, NPC.Center + drawOffset + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition,
            glowOrbFrame, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, glowOrbFrame.Size() / 2f, NPC.scale * 1.15f
            * MiscHelpers.BetterEssScale(2, 0.05f), SpriteEffects.None, 0f));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture, frameBody, NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => Main.EntitySpriteDraw(face, NPC.Center + drawOffset + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition, frameFace,
            Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() / 2f, NPC.scale, SpriteEffects.None));

        return false;
    }
}