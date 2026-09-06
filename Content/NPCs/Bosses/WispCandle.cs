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

    public ref float SpawnState => ref NPC.ai[0];
    public ref float WispID => ref NPC.ai[1];

    public int FlameState = 0;
    public int ExtraParticles = 0;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.MPAllowedEnemies[Type] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        Main.npcFrameCount[NPC.type] = 1;
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
    }

    public override void AI()
    {
        if (emitter != null)
            emitter.keptAlive = true;

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
                if (Main.rand.NextBool(3))
                {
                    emitter?.Emit(NPC.Top - new Vector2(0, 10f * NPC.scale),
                        (-Vector2.UnitY * Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.ToRadians(30)), 0f, 20);
                }
            }

            FlameState = 0;
            ExtraParticles = 0;
        }
    }

    public override bool? CanFallThroughPlatforms()
    {
        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Vector2 stretch = new(NPC.scale, NPC.scale);
        Texture2D tex = TextureAssets.Npc[NPC.type].Value;
        Vector2 origin = new(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
        Vector2 miragePos = NPC.Center - Main.screenPosition;

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
            spriteBatch.Draw(tex, miragePos + new Vector2(0f, 2f).RotatedBy(radians) * time, null, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
        }

        for (float i = 0f; i < 1f; i += 0.2f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            spriteBatch.Draw(tex, miragePos + new Vector2(0f, 4f).RotatedBy(radians) * time, null, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);
        }

        spriteBatch.Draw(tex, miragePos, null, Color.White * NPC.Opacity, NPC.rotation, origin, stretch, SpriteEffects.None, 0);

        return false;
    }
}