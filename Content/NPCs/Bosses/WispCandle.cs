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
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.NPCs.Bosses;

public class WispCandle : ModNPC
{
    public override string Texture => "ITD/Content/NPCs/Bosses/WispCandle";
    public static MiscShaderData Shader = new MiscShaderData(Main.VertexPixelShaderRef, "MagicMissile").UseProjectionMatrix(true);

    public VertexStrip TrailStrip = new VertexStrip();
    public ParticleEmitter emitter;
    public ParticleEmitter emitter2;

    public ref float SpawnState => ref NPC.ai[0];
    public ref float WispID => ref NPC.ai[1];

    public int FlameState = 0;
    public int ExtraParticles = 0;
    public float currentHandX = 30f;

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
        NPC.scale = 1.25f;
        emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        emitter.tag = NPC;
        emitter2 = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter2.tag = NPC;
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
        if (emitter != null) emitter.keptAlive = true;
        if (emitter2 != null) emitter2.keptAlive = true;
    }

    private Vector2[] trailOldPositions = new Vector2[40];
    private float[] trailOldRotations = new float[40];

    public override void AI()
    {
        if (emitter != null) emitter.keptAlive = true;
        if (emitter2 != null) emitter2.keptAlive = true;

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

        for (int i = trailOldPositions.Length - 1; i > 0; i--)
        {
            trailOldPositions[i] = trailOldPositions[i - 1];
            trailOldRotations[i] = trailOldRotations[i - 1];
        }
        trailOldPositions[0] = NPC.position;
        trailOldRotations[0] = NPC.rotation + MathHelper.PiOver2;

        if (SpawnState == 1)
        {
            NPC Wisp = MiscHelpers.NPCExists(WispID, ModContent.NPCType<MotherWisp>());

            if (Wisp == null)
            {
                NPC.active = false;
                if (Main.netMode != NetmodeID.MultiplayerClient) NPC.netUpdate = true;
                return;
            }

            NPC.spriteDirection = Wisp.Center.X > NPC.Center.X ? 1 : -1;

            float targetHandX = 30f * NPC.spriteDirection;
            currentHandX = MathHelper.Lerp(currentHandX, targetHandX, 0.15f);

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

                int flameCount = Main.rand.Next(3, 6);
                for (int i = 0; i < flameCount; i++)
                {
                    float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 3f;
                    emitter?.Emit(candleTop + Main.rand.NextVector2Circular(8f, 8f), new Vector2(wiggle, -Main.rand.NextFloat(4f, 8f)), 0f, 35);
                }

                int mistCount = Main.rand.Next(1, 3);
                for (int i = 0; i < mistCount; i++)
                {
                    float mistWiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 2f;
                    Vector2 mistVelocity = new Vector2(mistWiggle, -Main.rand.NextFloat(3f, 6f) * NPC.scale);
                    emitter?.Emit(candleTop + Main.rand.NextVector2Circular(10f, 10f) * NPC.scale, mistVelocity, 0f);
                }
            }

            FlameState = 0;
            ExtraParticles = 0;

            float targetRotation = 0f;

            if (NPC.localAI[0] == 3)
            {
                targetRotation = NPC.localAI[1];
            }

            NPC.rotation = Utils.AngleLerp(NPC.rotation, targetRotation, 0.15f);
        }
    }

    private Color StripColors(float progressOnStrip)
    {
        Color result = Color.Lerp(new Color(0, 0, 0, 0), new Color(131, 255, 236, 150), progressOnStrip);
        result.A /= 2;
        return result * NPC.Opacity;
    }

    private float StripWidth(float progressOnStrip)
    {
        return MathHelper.Lerp(40f, 1f, progressOnStrip);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Vector2 stretch = new(NPC.scale, NPC.scale);
        Texture2D tex = TextureAssets.Npc[NPC.type].Value;
        Vector2 origin = new(tex.Width / 2f, tex.Height / 2f / Main.npcFrameCount[NPC.type]);
        Vector2 miragePos = NPC.Center - Main.screenPosition;
        SpriteEffects effects = NPC.spriteDirection != -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        if (NPC.localAI[0] == 3)
        {
            GameShaders.Misc["LightDisc"].Apply(null);
            TrailStrip.PrepareStrip(trailOldPositions, trailOldRotations, StripColors, StripWidth, NPC.Size * 0.5f - Main.screenPosition, trailOldRotations.Length, true);
            TrailStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        float time = Main.GlobalTimeWrappedHourly;
        float timer = (float)Main.time / 240f + time * 0.04f;

        time %= 4f;
        time /= 2f;

        if (time >= 1f) time = 2f - time;
        time = time * 0.5f + 0.75f;

        for (float i = 0f; i < 1f; i += 0.1f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            sb.Draw(tex, miragePos + new Vector2(0f, 2f).RotatedBy(radians) * time, null, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, origin, stretch, effects, 0);
        }

        for (float i = 0f; i < 1f; i += 0.2f)
        {
            float radians = (i + timer) * MathHelper.TwoPi;
            sb.Draw(tex, miragePos + new Vector2(0f, 4f).RotatedBy(radians) * time, null, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, origin, stretch, effects, 0);
        }

        sb.Draw(tex, miragePos, null, Color.White * NPC.Opacity, NPC.rotation, origin, stretch, effects, 0);

        return false;
    }

    public override bool? CanFallThroughPlatforms()
    {
        return false;
    }
}