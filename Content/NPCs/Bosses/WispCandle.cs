using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Bosses;

public class WispCandle : ModNPC
{
    public ParticleEmitter emitter;
    public ParticleEmitter emitter2;

    public ref float SpawnState => ref NPC.ai[0];
    public ref float WispID => ref NPC.ai[1];

    public int FlameState = 0;
    public int ExtraParticles = 0;
    public float currentHandX = 20f;
    public bool showHand = false;
    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        Main.npcFrameCount[NPC.type] = 1;
        NPCID.Sets.TrailCacheLength[NPC.type] = 12;
        NPCID.Sets.TrailingMode[NPC.type] = 2;
    }

    public override void SetDefaults()
    {
        NPC.width = 24;
        NPC.height = 38;
        NPC.damage = 30;
        NPC.defense = 0;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit42;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.dontTakeDamage = true;
        NPC.aiStyle = -1;
        NPC.boss = true;
        NPC.hide = true;
        NPC.scale = 1.25f;
        emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = NPC; 
        emitter2 = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter2.tag = NPC;
    }

    public override void DrawBehind(int index)
    {
        Main.instance.DrawCacheNPCsOverPlayers.Add(index);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = 1000;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(FlameState);
        writer.Write(ExtraParticles);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        FlameState = reader.ReadInt32();
        ExtraParticles = reader.ReadInt32();
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (emitter != null)
            emitter.keptAlive = true;
        if (emitter2 != null)
            emitter2.keptAlive = true;
    }

    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;
        if (emitter2 != null)
            emitter2.keptAlive = true;

        if (SpawnState == 0)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int id = NPCHelpers.NewNPCEasy(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<MotherWisp>(), 0, NPC.whoAmI);
                WispID = id;
                SpawnState = 1;
                NPC.netUpdate = true;
            }
            return;
        }

        if (SpawnState == 1)
        {
            NPC Wisp = MiscHelpers.NPCExists(WispID, ModContent.NPCType<MotherWisp>());

            if (Wisp == null)
            {
                NPC.active = false;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.netUpdate = true;
                }
                return;
            }

            NPC.spriteDirection = Wisp.Center.X > NPC.Center.X ? 1 : -1;

            float targetHandX = 20f * NPC.spriteDirection;
            currentHandX = MathHelper.Lerp(currentHandX, targetHandX, 0.15f);

            showHand = Wisp.ai[1] > 0f;

            if (showHand)
            {
                Vector2 handWorldPos = NPC.Center + new Vector2(currentHandX, 6f) * NPC.scale;

                if (Main.rand.NextBool(3))
                {
                    int particleCount = Main.rand.Next(1, 3);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 5f)) * 2.5f;
                        Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(3f, 4.5f) * NPC.scale);
                        Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.2f, NPC.height / 2.2f) * NPC.scale;
                        emitter2?.Emit(handWorldPos + spawnOffset, mistVelocity, 0f);
                    }
                }
            }

            if (FlameState == 1)
            {
                int amount = ExtraParticles > 0 ? ExtraParticles : 8;
                for (int i = 0; i < amount; i++)
                {
                    float candleWiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 24f) + Main.rand.NextFloat(MathHelper.TwoPi)) * 3.5f;
                    Vector2 flameVel = new Vector2(candleWiggle, -Main.rand.NextFloat(14f, 22f));
                    emitter?.Emit(NPC.Top - new Vector2(0, 10f * NPC.scale), flameVel, 0f, 40);
                }
            }
            else if (FlameState == 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    float candleWiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 30f) + Main.rand.NextFloat(MathHelper.TwoPi)) * 5f;
                    Vector2 flameVel = new Vector2(candleWiggle, -Main.rand.NextFloat(10f, 18f));
                    emitter?.Emit(NPC.Center + Main.rand.NextVector2Circular(15f, 15f), flameVel, 0f, 50);
                }
            }
            else
            {
                Vector2 candleTop = NPC.Top - new Vector2(0, 10f * NPC.scale);

                if (Main.rand.NextBool(3))
                {
                    float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 1.5f;
                    emitter?.Emit(candleTop + Main.rand.NextVector2Circular(4f, 4f), new Vector2(wiggle, -Main.rand.NextFloat(2f, 4f)), 0f, 20);
                }

                int mistCount = 1;
                for (int i = 0; i < mistCount; i++)
                {
                    float mistWiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 1.5f;
                    Vector2 mistVelocity = new Vector2(mistWiggle, -Main.rand.NextFloat(2f, 4f) * NPC.scale);
                    emitter?.Emit(candleTop + Main.rand.NextVector2Circular(6f, 6f) * NPC.scale, mistVelocity, 0f);
                }
            }

            FlameState = 0;
            ExtraParticles = 0;
        }
    }
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 stretch = new(NPC.scale, NPC.scale);
        Texture2D tex = TextureAssets.Npc[NPC.type].Value;
        Vector2 origin = new(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
        Vector2 miragePos = NPC.Center - Main.screenPosition;
        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f)
        {
            time = 2f - time;
        }

        time = time * 0.5f + 0.75f;

        for (float i = 0f; i < 1f; i += 0.1f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            spriteBatch.Draw(tex, miragePos + new Vector2(0f, 2f).RotatedBy(radians) * time, null, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, origin, stretch, effects, 0);
        }

        for (float i = 0f; i < 1f; i += 0.2f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            spriteBatch.Draw(tex, miragePos + new Vector2(0f, 4f).RotatedBy(radians) * time, null, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, origin, stretch, effects, 0);
        }

        spriteBatch.Draw(tex, miragePos, null, Color.White * NPC.Opacity, NPC.rotation, origin, stretch, effects, 0);

        if (showHand)
        {
            Texture2D handTex = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/MotherWisp").Value;
            Texture2D glowOrb = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
            Vector2 handOffset = new Vector2(currentHandX, 6f) * NPC.scale;

            Rectangle glowOrbFrame = glowOrb.Frame(1, 1, 0, 0);
            Rectangle handFrame = handTex.Frame(1, 1, 0, 0);

            for (int i = 1; i < NPC.oldPos.Length; i++)
            {
                if (NPC.oldPos[i] == Vector2.Zero) continue;

                Vector2 oldCenter = NPC.oldPos[i] + NPC.Size / 2f;
                Vector2 oldHandDrawPos = oldCenter - Main.screenPosition + handOffset;
                Color trailColor = new Color(131, 255, 236, 80) * NPC.Opacity * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length);

                spriteBatch.Draw(handTex, oldHandDrawPos, handFrame, trailColor, NPC.rotation, handFrame.Size() / 2f, NPC.scale * 0.35f, effects, 0f);
            }
            

            Vector2 handDrawPos = miragePos + handOffset;
            spriteBatch.Draw(glowOrb, handDrawPos, glowOrbFrame, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, glowOrbFrame.Size() / 2f, NPC.scale * 0.65f * MiscHelpers.BetterEssScale(2, 0.05f), SpriteEffects.None, 0f);
            spriteBatch.Draw(handTex, handDrawPos, handFrame, Color.White * NPC.Opacity, NPC.rotation, handFrame.Size() / 2f, NPC.scale * 0.3f, effects, 0f);
        }

        return false;
    }

    public override bool? CanFallThroughPlatforms()
    {
        return false;
    }
}