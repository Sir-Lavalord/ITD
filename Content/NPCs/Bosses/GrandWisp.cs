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
        NPC.HitSound = SoundID.NPCHit42;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
    }

    public int OwnerIndex => (int)NPC.ai[0];

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance * bossAdjustment);
        NPC.damage = (int)(NPC.damage * 0.7f);
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

    public override void AI()
    {
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
            float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 2.5f;
            Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(8f, 10.5f) * NPC.scale);
            Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.2f, NPC.height / 2.2f) * NPC.scale;
            emitter?.Emit(NPC.Center + spawnOffset, mistVelocity, 0f);
        }

        if (NPC.ai[1] == 0)
        {
            NPC.TargetClosest(false);

            NPC candle = MiscHelpers.NPCExists((int)Mom.ai[0], ModContent.NPCType<WispCandle>());

            if (candle != null && candle.active)
            {
                int activeWisps = 0;
                int myIndex = 0;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC other = Main.npc[i];
                    if (other.active && other.type == ModContent.NPCType<GrandWisp>() && (int)other.ai[0] == OwnerIndex)
                    {
                        if (i == NPC.whoAmI) myIndex = activeWisps;
                        activeWisps++;
                    }
                }

                NPC.localAI[1] += 0.02f;

                float dynamicRadius = Math.Max(160f, activeWisps * 50f);
                float myAngle = NPC.localAI[1] + (MathHelper.TwoPi / Math.Max(1, activeWisps)) * myIndex;
                Vector2 targetPos = candle.Center + new Vector2(dynamicRadius, 0).RotatedBy(myAngle);

                NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.08f, 0.1f);
            }
            else
            {
                NPC.velocity *= 0.98f;
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
    }

    public override void OnKill()
    {
        NPC Mom = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<MotherWisp>());
        if (Mom == null) return;

        if (NPC.ai[1] == 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Mom.localAI[2] += 1f;
                Mom.netUpdate = true;
            }
        }
    }

    int faceFrameCurrent = 0;

    public override void FindFrame(int frameHeight)
    {
        if (++NPC.frameCounter >= 10)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y = (NPC.frame.Y + frameHeight) % (6 * frameHeight);
            if (faceFrameCurrent++ >= 2)
            {
                faceFrameCurrent = 0;
            }
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

        void DrawAtNPC(Texture2D tex, Rectangle rect, float scale)
        {
            sb.Draw(tex, NPC.Center + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, rect, Color.White * NPC.Opacity, NPC.rotation,
                rect.Size() / 2f, scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => Main.EntitySpriteDraw(glowOrb, NPC.Center + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition, 
            glowOrbFrame, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, glowOrbFrame.Size() / 2f, NPC.scale * 1.25f 
            * MiscHelpers.BetterEssScale(2, 0.05f), SpriteEffects.None, 0f));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture, frameBody, NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => Main.EntitySpriteDraw(face, NPC.Center + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition, frameFace,
            Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() / 2f, NPC.scale, SpriteEffects.None));

        return false;
    }
}