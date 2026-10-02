using ITD.Content.BossBars;
using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Content.Projectiles.Hostile.MotherWisp;
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

[AutoloadBossHead]
public class MotherWisp : ModNPC
{
    public override string Texture => "ITD/Content/NPCs/Bosses/MotherWisp";

    #region Fields & Properties
    public ParticleEmitter emitter;
    public ParticleEmitter handEmitter;

    public enum ActionState
    {
        Spawning,
        Idle,
        ChooseCombo,
        ExecuteCombo,
        Die,
        Stunned
    }

    public enum BaseAttack
    {
        None = -1,
        CandleMash = 0,
        Fireblow = 1,
        Enflame = 2,
        FlameSword = 3,
        Split = 4
    }

    public ref float AI_State => ref NPC.ai[1];
    public ref float MainAttack => ref NPC.ai[2];
    public ref float SecAttack => ref NPC.ai[3];

    public ref float AttackTimer => ref NPC.localAI[0];
    public ref float AttackCount => ref NPC.localAI[1];

    public int maxWispCount = 10;
    public float splitTimeDefault = 420;
    public float splitTime = 420;
    public int minWispCount = 1;

    public float GrandWispsLost
    {
        get => NPC.localAI[2];
        set => NPC.localAI[2] = value;
    }

    public Vector2 aimPos;
    public int CandleIndex => (int)NPC.ai[0];
    public bool HostCheck => Main.netMode != NetmodeID.MultiplayerClient;

    private int faceFrameTotal = 6;
    private int faceFrameCurrent = 0;
    private int faceFrameCounter = 0;
    private int consecutiveMainCount = 0;

    public Vector2 actualHandPos;
    public Vector2[] handOldPos = new Vector2[12];

    private bool expertMode = Main.expertMode;
    private bool masterMode = Main.masterMode;
    #endregion

    #region Initialization & Scaling
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 6;
    }

    public override void SetDefaults()
    {
        NPC.width = 124;
        NPC.height = 120;
        NPC.damage = 30;
        NPC.defense = 0;
        NPC.life = 1;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit37;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.boss = true;
        NPC.dontTakeDamage = true;
        NPC.BossBar = ModContent.GetInstance<WispBossBar>();
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;

        handEmitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        handEmitter.tag = NPC;
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (expertMode && !masterMode)
        {
            splitTime = 500;
            maxWispCount = 12;
            minWispCount = 2;
        }
        if (masterMode)
        {
            splitTime = 600;
            maxWispCount = 16;
            minWispCount = 3;
        }
        if (HostCheck)
        {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<WispArena>(), 0, 0, Main.myPlayer, NPC.whoAmI);
        }
        NPC.life = 1;
        base.OnSpawn(source);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        NPC.damage = (int)(NPC.damage * 0.7f);
    }
    #endregion

    #region Net Syncing & Checks
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(AttackTimer);
        writer.Write(AttackCount);
        writer.Write(aimPos.X);
        writer.Write(aimPos.Y);
        writer.Write(consecutiveMainCount);
        writer.Write(GrandWispsLost);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        AttackTimer = reader.ReadSingle();
        AttackCount = reader.ReadSingle();
        aimPos.X = reader.ReadSingle();
        aimPos.Y = reader.ReadSingle();
        consecutiveMainCount = reader.ReadInt32();
        GrandWispsLost = reader.ReadSingle();
    }

    public override bool CheckDead()
    {
        if (GrandWispsLost < maxWispCount)
        {
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            NPC.active = true;

            AI_State = (float)ActionState.ExecuteCombo;
            MainAttack = (float)BaseAttack.Split;
            SecAttack = -1;
            AttackTimer = 0;
            AttackCount = 0;
            NPC.netUpdate = true;

            return false;
        }
        return true;
    }
    #endregion

    #region Helper Methods
    public int ProjectileDamage(int damage)
    {
        if (expertMode) return (int)(damage / 2.5f);
        if (masterMode) return (int)(damage / 3.5f);
        return damage;
    }

    public void AnimateFace(int frameStart, int frameEnd, int frameSpeed, bool doLoop = true, bool reversed = false)
    {
        if (frameStart != -1)
        {
            bool inBounds;
            if (!reversed)
            {
                if (frameStart <= frameEnd)
                    inBounds = faceFrameCurrent >= frameStart && faceFrameCurrent <= frameEnd;
                else
                    inBounds = faceFrameCurrent >= frameStart || faceFrameCurrent <= frameEnd;
            }
            else
            {
                if (frameStart >= frameEnd)
                    inBounds = faceFrameCurrent <= frameStart && faceFrameCurrent >= frameEnd;
                else
                    inBounds = faceFrameCurrent <= frameStart || faceFrameCurrent >= frameEnd;
            }

            if (!inBounds)
            {
                faceFrameCurrent = frameStart;
                faceFrameCounter = 0;
            }
        }

        if (++faceFrameCounter >= frameSpeed)
        {
            faceFrameCounter = 0;

            if (faceFrameCurrent == frameEnd)
            {
                if (doLoop)
                {
                    if (frameStart == -1)
                        faceFrameCurrent = reversed ? faceFrameTotal - 1 : 0;
                    else
                        faceFrameCurrent = frameStart;
                }
            }
            else
            {
                if (!reversed)
                {
                    faceFrameCurrent++;
                    if (faceFrameCurrent >= faceFrameTotal)
                        faceFrameCurrent = 0;
                }
                else
                {
                    faceFrameCurrent--;
                    if (faceFrameCurrent < 0)
                        faceFrameCurrent = faceFrameTotal - 1;
                }
            }
        }
    }

    private void ApplyFriction(Vector2? targetPos = null, float speed = 0.05f, float friction = 0.8f)
    {
        if (targetPos.HasValue)
        {
            Vector2 diff = targetPos.Value - NPC.Center;
            NPC.velocity += diff * speed;
        }

        NPC.velocity *= friction;

        if (NPC.velocity.LengthSquared() < 0.01f)
            NPC.velocity = Vector2.Zero;
    }

    private void SetCandleState(NPC candle, int state, float angle = 0f)
    {
        candle.localAI[0] = state;
        candle.localAI[1] = angle;
    }

    private void ResetState(ActionState nextState)
    {
        if (NPC.life <= 1 && GrandWispsLost < maxWispCount)
        {
            AI_State = (float)ActionState.ExecuteCombo;
            MainAttack = (float)BaseAttack.Split;
            SecAttack = -1;
            AttackTimer = 0;
            AttackCount = 0;
            NPC.netUpdate = true;
            return;
        }

        SetArenaRadius(0);
        AI_State = (float)nextState;
        AttackTimer = 0;
        AttackCount = 0;
        MainAttack = -1;
        SecAttack = -1;
        NPC.netUpdate = true;
    }

    public void SetArenaRadius(float newRadius)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.type == ModContent.ProjectileType<WispArena>() && (int)p.ai[0] == NPC.whoAmI)
            {
                p.ai[1] = newRadius;
                p.netUpdate = true;
            }
        }
    }

    public void SetArenaRadius(float newRadius, bool isLocked = false, Vector2 lockedPos = default)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.type == ModContent.ProjectileType<WispArena>() && (int)p.ai[0] == NPC.whoAmI)
            {
                p.ai[1] = newRadius;
                p.ai[2] = isLocked ? 1f : 0f;
                if (isLocked)
                {
                    p.localAI[1] = lockedPos.X;
                    p.localAI[2] = lockedPos.Y;
                }
                p.netUpdate = true;
            }
        }
    }
    private void GeneralHover(Player player, float hoverHeight = 300f, float speed = 0.05f)
    {
        float verticalBob = MiscHelpers.BetterEssScale(2, 0.2f);
        Vector2 hoverTarget = player.Center - new Vector2(0, hoverHeight * verticalBob);
        NPC.velocity = (hoverTarget - NPC.Center) * speed;
    }

    private void CandleIdleHover(NPC candle)
    {
        Vector2 targetPos = NPC.Center + new Vector2(0, 200f);
        candle.velocity = (targetPos - candle.Center) * 0.1f;
    }

    private void DoEnflameAnimation(float timer, NPC candle, bool isWindup)
    {
        int extraParticles = (int)MathHelper.Min(timer / 3f, 12f);

        for (int i = 0; i < extraParticles; i++)
        {
            float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 24f) + Main.rand.NextFloat(MathHelper.TwoPi)) * 3.5f;
            Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(14f, 22f) * NPC.scale);
            Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.5f, NPC.height / 2.5f) * NPC.scale;
            emitter?.Emit(NPC.Center + spawnOffset, mistVelocity, 0f);
        }

        if (candle.ModNPC is WispCandle wispCandle)
        {
            wispCandle.FlameState = isWindup ? 1 : 2;
            wispCandle.ExtraParticles = extraParticles;
        }
    }
    #endregion

    #region Main AI Core
    public override void AI()
    {
        if (emitter != null) emitter.keptAlive = true;
        if (handEmitter != null) handEmitter.keptAlive = true;

        NPC candle = MiscHelpers.NPCExists(CandleIndex, ModContent.NPCType<WispCandle>());
        if (candle == null)
        {
            NPC.active = false;
            return;
        }

        if (handOldPos[0] == Vector2.Zero)
        {
            actualHandPos = NPC.Center;
            for (int i = 0; i < handOldPos.Length; i++) handOldPos[i] = NPC.Center;
        }

        SetCandleState(candle, 0);

        GrandWispsLost = MathHelper.Clamp(GrandWispsLost, 0, maxWispCount);

        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();

        Player player = Main.player[NPC.target];

        if (MainAttack != (float)BaseAttack.Split)
        {
            NPC.Opacity = 1f;
            NPC.dontTakeDamage = false;
            NPC.ShowNameOnHover = true;
        }

        EmitAmbientParticles();

        switch ((ActionState)AI_State)
        {
            case ActionState.Spawning:
                HandleSpawningState(candle);
                break;

            case ActionState.Idle:
                HandleIdleState(player, candle);
                break;

            case ActionState.Stunned:
                HandleStunnedState(player, candle);
                break;

            case ActionState.ChooseCombo:
                HandleComboSelection();
                break;

            case ActionState.ExecuteCombo:
                ExecuteComboAttack(player, candle);
                break;
        }

        UpdateHandPosition(candle);
        EmitHandParticles();
        UpdateBossRotation();
    }

    private void EmitAmbientParticles()
    {
        int particleCount = Main.rand.Next(4, 7);
        for (int i = 0; i < particleCount; i++)
        {
            if (NPC.Opacity <= 0f) continue;

            float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 2.5f;
            Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(8f, 10.5f) * NPC.scale);
            Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.2f, NPC.height / 2.2f) * NPC.scale;
            emitter?.Emit(NPC.Center + spawnOffset, mistVelocity, 0f);
        }
    }

    private void UpdateHandPosition(NPC candle)
    {
        if (AI_State != (float)ActionState.Spawning && MainAttack != (float)BaseAttack.Split)
        {
            if (candle.ModNPC is WispCandle wispCandle)
            {
                actualHandPos = candle.Center + new Vector2(wispCandle.currentHandX, 8f) * candle.scale;
            }
        }

        for (int i = handOldPos.Length - 1; i > 0; i--)
            handOldPos[i] = handOldPos[i - 1];
        handOldPos[0] = actualHandPos;
    }

    private void EmitHandParticles()
    {
        if (Main.rand.NextBool(3) && NPC.Opacity > 0f)
        {
            int pCount = Main.rand.Next(1, 3);
            for (int i = 0; i < pCount; i++)
            {
                float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 5f)) * 2.5f;
                Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(3f, 4.5f) * NPC.scale);
                Vector2 handSpawnOffset = Main.rand.NextVector2Circular(12f, 12f) * NPC.scale;
                handEmitter?.Emit(actualHandPos + handSpawnOffset, mistVelocity, 0f);
            }
        }
    }

    private void UpdateBossRotation()
    {
        float targetBossRotation = 0f;
        if (AI_State == (float)ActionState.Idle || AI_State == (float)ActionState.Spawning || AI_State == (float)ActionState.Stunned)
        {
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            targetBossRotation = rotationFactor * maxRotation;
        }
        NPC.rotation = Utils.AngleLerp(NPC.rotation, targetBossRotation, 0.15f);
    }
    #endregion

    #region State Machine Logic
    private void HandleSpawningState(NPC candle)
    {
        NPC.dontTakeDamage = true;
        AnimateFace(0, 5, 6);

        float morphTime = 120f;
        float swoopTime = 25f;
        float holdTime = 10f;
        float recoilTime = 30f;
        float totalTime = morphTime + swoopTime + holdTime + recoilTime;

        if (AttackTimer == 0) aimPos = candle.Center;

        float progress = Utils.Clamp(AttackTimer / morphTime, 0f, 1f);
        NPC.scale = MathHelper.Lerp(0f, 1.5f, progress);

        Vector2 hoverPos = aimPos + new Vector2(0, -160f * NPC.scale);
        NPC.Center = Vector2.Lerp(NPC.Center, hoverPos, 0.1f);
        ApplyFriction();

        if (candle.ModNPC is WispCandle wispCandleSpawn)
        {
            Vector2 startHandPos = NPC.Center + new Vector2(NPC.spriteDirection * 120f, -40f) * NPC.scale;
            Vector2 idleCandlePos = NPC.Center + new Vector2(0, 160f);
            Vector2 handleOffset = new Vector2(wispCandleSpawn.currentHandX, 8f) * candle.scale;
            Vector2 targetHandle = idleCandlePos + handleOffset;

            if (AttackTimer < morphTime)
            {

                NPC.life += (int)(NPC.lifeMax / morphTime);
                NPC.life = (int)MathHelper.Clamp(NPC.life, 0, NPC.lifeMax);
                actualHandPos = startHandPos;
                candle.Center = aimPos;
                candle.velocity = Vector2.Zero;
                SetCandleState(candle, 0, 0f);
            }
            else if (AttackTimer < morphTime + swoopTime)
            {
                float localT = (AttackTimer - morphTime) / swoopTime;
                float easedT = 1f - (float)Math.Pow(1f - localT, 3);

                Vector2 p0 = startHandPos;
                Vector2 p2 = NPC.Center + new Vector2(-NPC.spriteDirection * 100f, 20f) * NPC.scale;
                Vector2 p1 = aimPos + new Vector2(NPC.spriteDirection * 50f, 100f);

                Vector2 q0 = Vector2.Lerp(p0, p1, easedT);
                Vector2 q1 = Vector2.Lerp(p1, p2, easedT);
                actualHandPos = Vector2.Lerp(q0, q1, easedT);

                if (easedT > 0.45f)
                {
                    candle.Center = actualHandPos - handleOffset;
                    Vector2 handVelocity = actualHandPos - handOldPos[0];
                    float targetRotation = -MathHelper.Clamp(handVelocity.X * 0.05f, -1.2f, 1.2f);
                    SetCandleState(candle, 4, targetRotation);
                }
                else
                {
                    candle.Center = aimPos;
                    SetCandleState(candle, 0, 0f);
                }
                candle.velocity = Vector2.Zero;
            }
            else if (AttackTimer < morphTime + swoopTime + holdTime)
            {
                Vector2 p2 = NPC.Center + new Vector2(-NPC.spriteDirection * 100f, 20f) * NPC.scale;
                actualHandPos = p2;
                candle.Center = actualHandPos - handleOffset;
                candle.velocity = Vector2.Zero;
            }
            else
            {
                float localT = (AttackTimer - (morphTime + swoopTime + holdTime)) / recoilTime;
                float eased = MathHelper.SmoothStep(0f, 1f, localT);

                Vector2 recoilStart = NPC.Center + new Vector2(-NPC.spriteDirection * 100f, 20f) * NPC.scale;

                actualHandPos = Vector2.Lerp(recoilStart, targetHandle, eased);
                candle.Center = actualHandPos - handleOffset;
                candle.velocity = Vector2.Zero;
                SetCandleState(candle, 0, 0f);
            }
        }

        if (AttackTimer++ >= totalTime)
        {
            NPC.dontTakeDamage = false;
            ResetState(ActionState.Idle);
        }
    }

    private void HandleIdleState(Player player, NPC candle)
    {
        AnimateFace(-1, 0, 6, false);
        AttackTimer++;
        GeneralHover(player, 300f);
        CandleIdleHover(candle);

        if (AttackTimer >= 180)
        {
            ResetState(ActionState.ChooseCombo);
        }
    }

    private void HandleStunnedState(Player player, NPC candle)
    {
        AnimateFace(3, 5, 6);
        GeneralHover(player, 300f);
        CandleIdleHover(candle);

        float stunDuration = 120f;
        if (AttackTimer++ >= stunDuration)
        {
            ResetState(ActionState.Idle);
        }
    }

    //warp
    private void HandleComboSelection()
    {
        if (HostCheck)
        {
            float prevMain = MainAttack;
            float prevSec = SecAttack;

            MainAttack = Main.rand.Next(3);
            if (MainAttack == prevMain && consecutiveMainCount >= 2)
                MainAttack = (MainAttack + Main.rand.Next(1, 3)) % 3;

            SecAttack = Main.rand.Next(3);

            if (SecAttack == MainAttack)
                SecAttack = (MainAttack + 1) % 3;

            while (MainAttack == prevMain && SecAttack == prevSec)
            {
                SecAttack = Main.rand.Next(3);
                if (SecAttack == MainAttack) Main.rand.Next(3);
            }

            if (MainAttack == prevMain) consecutiveMainCount++;
            else consecutiveMainCount = 1;

            AI_State = (float)ActionState.ExecuteCombo;
            NPC.netUpdate = true;
        }
    }

    private void ExecuteComboAttack(Player player, NPC candle)
    {
        switch ((BaseAttack)MainAttack)
        {
            case BaseAttack.CandleMash: CandleMash(player, candle, (BaseAttack)SecAttack); break;
            case BaseAttack.Fireblow: Fireblow(player, candle, (BaseAttack)SecAttack); break;
            case BaseAttack.Enflame: Enflame(player, candle, (BaseAttack)SecAttack); break;
            case BaseAttack.Split: SplitAttack(player, candle); break;
        }
    }
    #endregion

    #region Attacks
    private void SplitAttack(Player player, NPC candle)
    {
        AttackTimer++;
        if (AttackTimer == 1)
        {
            if (GrandWispsLost > maxWispCount - minWispCount)
            {
                GrandWispsLost = maxWispCount - minWispCount;
                NPC.netUpdate = true;
            }
        }

        float shakeEnd = 120f;
        float danceTime = 420f;

        candle.velocity *= 0.8f;
        if (candle.velocity.Length() < 0.1f) candle.velocity = Vector2.Zero;

        if (AttackTimer < shakeEnd)
        {
            if (AttackTimer <= 90) AnimateFace(3, 5, 6, true);
            else AnimateFace(-1, 5, 6, false);

            Vector2 targetPos = new Vector2(candle.Center.X, candle.Top.Y - (NPC.height) * NPC.scale);
            NPC.Center = Vector2.Lerp(NPC.Center, targetPos, 0.1f);

            if (candle.ModNPC is WispCandle wispCandle)
            {
                actualHandPos = candle.Center + new Vector2(wispCandle.currentHandX, 8f) * candle.scale;
            }
        }
        else
        {
            NPC.dontTakeDamage = true;
            NPC.Opacity = 0f;
            NPC.ShowNameOnHover = false;
            NPC.velocity = Vector2.Zero;

            NPC.Center = candle.Center + new Vector2(0, -50);

            if (AttackTimer == shakeEnd)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

                int countToSpawn = maxWispCount - (int)GrandWispsLost;

                if (HostCheck)
                {
                    for (int i = 0; i < countToSpawn; i++)
                    {
                        int wispID = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<GrandWisp>(), 0, NPC.whoAmI);
                        if (Main.npc[wispID].active)
                        {
                            Main.npc[wispID].velocity = (Vector2.UnitY * Main.rand.NextFloat(6f, 10f)).RotatedByRandom(MathHelper.ToRadians(360));
                            Main.npc[wispID].netUpdate = true;
                        }
                    }
                }
            }

            bool forceReform = (AttackTimer - shakeEnd) >= danceTime;
            bool anyWispsAlive = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<GrandWisp>() && (int)Main.npc[i].ai[0] == NPC.whoAmI)
                {
                    anyWispsAlive = true;
                    if (forceReform && Main.npc[i].ai[1] == 0)
                    {
                        Main.npc[i].ai[1] = 1;
                        Main.npc[i].netUpdate = true;
                    }
                }
            }

            if (!anyWispsAlive)
            {
                if (forceReform || AttackTimer > shakeEnd + 5f)
                {
                    NPC.Opacity = 1f;

                    if (GrandWispsLost >= maxWispCount)
                    {
                        if (HostCheck) NPC.StrikeInstantKill();
                        return;
                    }

                    for (int i = 0; i <= 18; i++)
                    {
                        emitter?.Emit(NPC.Center, (-Vector2.UnitY * Main.rand.NextFloat(7f, 10f)).RotatedByRandom(MathHelper.ToRadians(360)), 0f, 90);
                    }

                    NPC.dontTakeDamage = false;
                    NPC.ShowNameOnHover = true;
                    NPC.life = NPC.lifeMax;

                    ResetState(ActionState.Stunned);
                }
            }
        }
    }

    private void CandleMash(Player player, NPC candle, BaseAttack sec)
    {
        AnimateFace(0, 5, 6);
        AttackTimer++;
        ApplyFriction(player.Center - new Vector2(0, 300f));

        switch (sec)
        {
            case BaseAttack.None: CandleMash_None(player, candle); break;
            case BaseAttack.Fireblow: CandleMash_Fireblow(player, candle); break;
            case BaseAttack.Enflame: CandleMash_Enflame(player, candle); break;
        }
    }

    private void CandleMash_None(Player player, NPC candle)
    {
        float windupEnd = 40f;
        float positionEnd = 60f;
        float attackTimeout = 120f;
        float restEnd = 150f;

        if (AttackTimer < windupEnd)
        {
            Vector2 handPos = NPC.Center + new Vector2(NPC.direction * 180, 50);
            candle.Center = Vector2.Lerp(candle.Center, handPos, 0.15f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer < positionEnd)
        {
            Vector2 targetAim = player.Center - new Vector2(0, 250);
            candle.Center = Vector2.Lerp(candle.Center, targetAim, 0.2f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer == positionEnd)
        {
            candle.velocity = new Vector2(0, 25f);
            candle.netUpdate = true;
        }
        else if (AttackTimer > positionEnd && AttackTimer <= attackTimeout)
        {
            bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
            bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

            if (hitTile || hitFloor || AttackTimer == attackTimeout)
            {
                candle.velocity = Vector2.Zero;
                SoundEngine.PlaySound(SoundID.Item14, candle.Center);

                if (HostCheck)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Bottom + new Vector2(30 * j, -20), new Vector2(8 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), ProjectileDamage(ProjectileDamage((int)(NPC.damage * 0.5f))), 0, -1);
                    }
                }

                AttackTimer = attackTimeout;
            }
        }
        else if (AttackTimer > attackTimeout && AttackTimer < restEnd)
        {
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer >= restEnd)
        {
            AttackCount++;
            if (AttackCount >= 3) ResetState(ActionState.Idle);
            else AttackTimer = 0;
        }
    }

    private void CandleMash_Fireblow(Player player, NPC candle)
    {
        AnimateFace(3, 5, 6, true);

        float windupEnd = 60f;
        float sweepDuration = 15f;
        float stopDuration = 5f;
        float cycleTime = sweepDuration + stopDuration;
        float orbGap = 90f;
        float maxSweepWidth = 1000f;

        int orbsPerSweep = (int)(maxSweepWidth / orbGap) + 2;
        float sweepWidth = (orbsPerSweep - 1) * orbGap;
        int maxSweeps = 4;

        if (AttackTimer < windupEnd)
        {
            Vector2 bossHoverPos = new Vector2(player.Center.X, player.Center.Y - 350f);
            NPC.Center = Vector2.Lerp(NPC.Center, bossHoverPos, 0.08f);
            NPC.velocity = Vector2.Zero;

            if (AttackTimer % 4 == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 offset = (MathHelper.TwoPi / 5 * i - MathHelper.PiOver2).ToRotationVector2() * (windupEnd - AttackTimer) * 2.5f;
                    Dust.NewDustPerfect(NPC.Center + offset, DustID.PurpleCrystalShard, -offset * 0.05f).noGravity = true;
                }
            }

            if (AttackTimer == windupEnd - 1) aimPos = player.Center - new Vector2(0, 500f);
        }
        else
        {
            float activeTime = AttackTimer - windupEnd;
            float timeInSweep = activeTime % cycleTime;

            int currentSweep = (int)(activeTime / cycleTime);
            if (currentSweep >= maxSweeps)
            {
                ResetState(ActionState.Idle);
                return;
            }

            bool sweepingRight = (currentSweep % 2 == 0);
            Vector2 sweepOffset = new Vector2(sweepWidth / 2f, 0);

            Vector2 leftEdge = aimPos - sweepOffset;
            Vector2 rightEdge = aimPos + sweepOffset;
            Vector2 edgeStart = sweepingRight ? leftEdge : rightEdge;
            Vector2 edgeEnd = sweepingRight ? rightEdge : leftEdge;

            if (timeInSweep <= sweepDuration)
            {
                float lerpFactor = timeInSweep / sweepDuration;
                candle.Center = Vector2.Lerp(edgeStart, edgeEnd, lerpFactor);
                candle.velocity = Vector2.Zero;

                float previousLerp = Math.Max(0f, timeInSweep - 1f) / sweepDuration;
                int startIndex = (timeInSweep == 0) ? 0 : (int)(previousLerp * (orbsPerSweep - 1)) + 1;
                int endIndex = (int)(lerpFactor * (orbsPerSweep - 1));

                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (HostCheck)
                    {
                        float exactLerp = i / (float)(orbsPerSweep - 1);
                        Vector2 exactSpawnPos = Vector2.Lerp(edgeStart, edgeEnd, exactLerp) - new Vector2(0, 20f);

                        float chevronIndex = i - (orbsPerSweep / 2f);
                        float sectorLean = MathHelper.ToRadians(10);
                        float sweepAngle = MathHelper.PiOver2 + (sweepingRight ? -sectorLean : sectorLean);

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), exactSpawnPos, Vector2.Zero, ModContent.ProjectileType<WispChevronBullet>(), ProjectileDamage(ProjectileDamage((int)(NPC.damage * 0.5f))), 0, Main.myPlayer, sweepAngle, chevronIndex, currentSweep);
                    }
                }
            }
            else
            {
                candle.Center = edgeEnd;
                candle.velocity = Vector2.Zero;
                aimPos.X = MathHelper.Lerp(aimPos.X, player.Center.X, 0.015f);
            }
        }
    }

    private void CandleMash_Enflame(Player player, NPC candle)
    {
        float windupEnd = 60f;
        float positionEnd = 90f;
        float attackTimeout = 180f;
        float restEnd = 250f;

        if (AttackTimer < attackTimeout) DoEnflameAnimation(AttackTimer, candle, AttackTimer < windupEnd);

        if (AttackTimer < windupEnd)
        {
            Vector2 handPos = NPC.Center + new Vector2(NPC.direction * 180, 50);
            candle.Center = Vector2.Lerp(candle.Center, handPos, 0.1f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer < positionEnd)
        {
            AnimateFace(-1, 5, 6, false);
            Vector2 targetAim = player.Center - new Vector2(0, 250);
            candle.Center = Vector2.Lerp(candle.Center, targetAim, 0.1f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer == positionEnd)
        {
            candle.velocity = new Vector2(0, 25f);
            candle.netUpdate = true;
        }
        else if (AttackTimer > positionEnd && AttackTimer <= attackTimeout)
        {
            AnimateFace(-1, 2, 3, false);

            bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
            bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

            if (hitTile || hitFloor || AttackTimer == attackTimeout)
            {
                candle.velocity = Vector2.Zero;
                SoundEngine.PlaySound(SoundID.Item14, candle.Center);

                if (HostCheck)
                {
                    int projType = ModContent.ProjectileType<WispFireBall>();
                    int gapStart = Main.rand.Next(-7, 8);

                    for (int i = -10; i <= 10; i++)
                    {
                        if (i >= gapStart && i <= gapStart + 1) continue;

                        float angle = MathHelper.Lerp(-MathHelper.PiOver2, 0f, i / 20f);
                        angle += Main.rand.NextFloat(-0.05f, 0.05f);
                        Vector2 shootVel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 15f);
                        shootVel.X *= 2f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Bottom + new Vector2(0, -20f), shootVel, projType, ProjectileDamage((int)(NPC.damage * 0.75f)), 0, -1);
                    }
                }
                AttackTimer = attackTimeout;
            }
        }
        else if (AttackTimer > attackTimeout && AttackTimer < restEnd)
        {
            AnimateFace(-1, 2, 3, false);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer >= restEnd)
        {
            AttackCount++;
            if (AttackCount >= 3) ResetState(ActionState.Idle);
            else AttackTimer = 0;
        }
    }

    private void Fireblow(Player player, NPC candle, BaseAttack sec)
    {
        AnimateFace(0, 5, 6);
        AttackTimer++;

        float hoverHeight = sec == BaseAttack.Enflame ? 600f : 300f;
        if (AttackTimer < 40f)
        {
            GeneralHover(player, hoverHeight);
        }
        else
        {
            ApplyFriction(player.Center - new Vector2(0, hoverHeight));
        }

        switch (sec)
        {
            case BaseAttack.None: Fireblow_None(player, candle); break;
            case BaseAttack.CandleMash: Fireblow_CandleMash(player, candle); break;
            case BaseAttack.Enflame: Fireblow_Enflame(player, candle); break;
        }
    }

    private void Fireblow_None(Player player, NPC candle)
    {
        float windupEnd = 40f;
        float blowEnd = 80f;
        float resetTime = 150f;

        if (AttackTimer < windupEnd) aimPos = player.Center;
        Vector2 aimDir = NPC.DirectionTo(aimPos);

        if (AttackTimer < windupEnd)
        {
            Vector2 targetPos = NPC.Center + aimDir * 50f;
            candle.Center = Vector2.Lerp(candle.Center, targetPos, 0.2f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer == windupEnd)
        {
            NPC.netUpdate = true;
            if (HostCheck)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, Vector2.Zero, ModContent.ProjectileType<WispFireBreathTelegraph>(), 0, 0, Main.myPlayer, aimDir.ToRotation(), candle.whoAmI);
            }
        }
        else if (AttackTimer > windupEnd && AttackTimer <= blowEnd)
        {
            Vector2 targetPos = NPC.Center + aimDir * 100f;
            candle.Center = targetPos;
            candle.velocity = Vector2.Zero;

            if (AttackTimer % 2 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                if (HostCheck)
                {
                    float spread = MathHelper.ToRadians(25);
                    Vector2 shootVel = aimDir.RotatedByRandom(spread) * Main.rand.NextFloat(8f, 12f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, ModContent.ProjectileType<WispFireBreath>(), ProjectileDamage(ProjectileDamage((int)(NPC.damage * 0.5f))), 0, -1);
                }
            }
        }
        else if (AttackTimer >= resetTime)
        {
            ResetState(ActionState.Idle);
        }
    }

    private void Fireblow_CandleMash(Player player, NPC candle)
    {
        int maxSlams = 4;
        float positionEnd = 60f;
        float ringDelay = 60f;
        float restEnd = 40f;

        if (AttackTimer == 1 && AttackCount == 0)
        {
            SetArenaRadius(500f, true, NPC.Center);
            candle.localAI[2] = 0f;
        }

        NPC.velocity = Vector2.Zero;

        if (AttackTimer < positionEnd)
        {
            Lighting.AddLight(candle.Center, 0.8f, 0.4f, 0f);
            if (candle.ModNPC is WispCandle wispCandle) wispCandle.FlameState = 2;

            Vector2 aimPosSmash = player.Bottom - new Vector2(0, 300f);
            candle.Center = Vector2.Lerp(candle.Center, aimPosSmash, 0.04f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer == positionEnd)
        {
            candle.velocity = new Vector2(0, 35f);
            candle.netUpdate = true;
        }
        else if (AttackTimer > positionEnd)
        {
            if (candle.localAI[2] == 0f)
            {
                bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
                bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

                if (hitTile || hitFloor || AttackTimer > positionEnd + 90f)
                {
                    candle.velocity = Vector2.Zero;
                    SoundEngine.PlaySound(SoundID.Item14, candle.Center);
                    candle.localAI[2] = 1f;
                    AttackTimer = positionEnd;
                    Projectile Blast = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), candle.Bottom, Vector2.Zero, ModContent.ProjectileType<WispTearBlast>(), ProjectileDamage((int)(NPC.damage * 0.5f)), 0, Main.myPlayer);
                    Blast.ai[1] = 200f;
                    Blast.localAI[1] = Main.rand.NextFloat(0.15f, 0.25f);
                    Blast.netUpdate = true;
                }
            }
            else if (candle.localAI[2] == 1f)
            {
                candle.velocity = Vector2.Zero;

                if (AttackTimer >= positionEnd + ringDelay)
                {
                    if (HostCheck)
                    {
                        int projType = ModContent.ProjectileType<WispSharpTear>();
                        int ringCount = 20;
                        float radius = 60f;

                        for (int i = 0; i < ringCount; i++)
                        {
                            Vector2 offset = (MathHelper.TwoPi / ringCount * i).ToRotationVector2() * radius;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center + offset, Vector2.Zero, projType, ProjectileDamage((int)(NPC.damage * 0.5f)), 0, Main.myPlayer, offset.ToRotation(), 0f);
                        }
                    }
                    candle.localAI[2] = 2f;
                    AttackTimer = positionEnd + ringDelay;
                }
            }
            else if (candle.localAI[2] == 2f)
            {
                candle.velocity = Vector2.Zero;

                if (AttackTimer >= positionEnd + ringDelay + restEnd)
                {
                    AttackCount++;
                    if (AttackCount >= maxSlams)
                    {
                        SetArenaRadius(1000f, false);
                        ResetState(ActionState.Idle);
                    }
                    else
                    {
                        AttackTimer = 0;
                        candle.localAI[2] = 0f;
                    }
                }
            }
        }
    }
    private void Fireblow_Enflame(Player player, NPC candle)
    {
        float chargeEnd = 60f;
        float telegraphEnd = 120f;
        float blowEnd = 260f;
        float sweepEnd = 360f;
        float restEnd = 360f;
        float hoverHeight = 800f;

        if (AttackTimer == 1) SetArenaRadius(400f, true, player.Center);

        Vector2 mouthOffset = new Vector2(0f, 60f) * candle.scale;

        if (AttackTimer < chargeEnd)
        {
            DoEnflameAnimation(AttackTimer, candle, true);
            ApplyFriction(player.Center - new Vector2(0f, hoverHeight), 0.08f, 0.8f);
            CandleIdleHover(candle);

            if (AttackTimer == chargeEnd - 1)
            {
                aimPos = player.Center;
                NPC.netUpdate = true;
            }
        }
        else if (AttackTimer <= telegraphEnd)
        {
            if (AttackTimer > chargeEnd)
            {
                NPC.velocity *= 0.8f;
            }
            aimPos = Vector2.Lerp(aimPos, player.Center, 0.05f);

            Vector2 targetHandle = NPC.Center + mouthOffset;
            candle.velocity = (targetHandle - candle.Center) * 0.2f;
            SetCandleState(candle, 0, 0f);

            DoEnflameAnimation(AttackTimer, candle, true);

            if (AttackTimer == chargeEnd)
            {
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<WispFireBreathTelegraph>(), 0, 0, Main.myPlayer, NPC.DirectionTo(aimPos).ToRotation(), candle.whoAmI);
                }
            }
        }
        else if (AttackTimer <= blowEnd)
        {
            NPC.velocity = Vector2.Zero;
            aimPos = Vector2.Lerp(aimPos, player.Center, 0.02f);

            Vector2 targetHandle = NPC.Center + mouthOffset;
            candle.velocity = (targetHandle - candle.Center) * 0.2f;
            SetCandleState(candle, 0, 0f);

            DoEnflameAnimation(AttackTimer, candle, false);
            AnimateFace(4, 5, 6, true);

            if (AttackTimer % 4 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                if (HostCheck)
                {
                    float spread = MathHelper.ToRadians(10);
                    Vector2 shootVel = NPC.DirectionTo(aimPos).RotatedByRandom(spread) * Main.rand.NextFloat(14f, 18f);
                    float targetY = aimPos.Y + 250f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ModContent.ProjectileType<WispFireburst>(), ProjectileDamage((int)(NPC.damage * 0.5f)), 0, Main.myPlayer, sweepEnd - AttackTimer, targetY);
                }
            }
        }
        else if (AttackTimer <= sweepEnd)
        {
            AnimateFace(-1, 0, 6, false);
            NPC.velocity = Vector2.Zero;
            CandleIdleHover(candle);
        }
        else if (AttackTimer >= restEnd)
        {
            SetArenaRadius(1000f, false);
            ResetState(ActionState.Idle);
        }
    }
    private void Enflame(Player player, NPC candle, BaseAttack sec)
    {
        AttackTimer++;

        CandleIdleHover(candle);

        float hoverHeight = 400f;
        if (sec == BaseAttack.Fireblow) hoverHeight = 400f;
        else if (sec == BaseAttack.CandleMash) hoverHeight = 300f;

        ApplyFriction(player.Center - new Vector2(0, hoverHeight), 0.05f, 0.8f);

        switch (sec)
        {
            case BaseAttack.None: Enflame_None(player, candle); break;
            case BaseAttack.CandleMash: Enflame_CandleMash(player, candle); break;
            case BaseAttack.Fireblow: Enflame_Fireblow(player, candle); break;
        }
    }

    private void Enflame_None(Player player, NPC candle)
    {
        if (AttackTimer < 120) DoEnflameAnimation(AttackTimer, candle, true);

        if (AttackTimer > 40 && AttackTimer < 120)
        {
            if (AttackTimer % 6 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20, candle.Center);

                if (HostCheck)
                {
                    Vector2 tipPos = candle.Top - new Vector2(0, 20f);
                    Vector2 shootVel = new Vector2(Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-14f, -9f));
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), tipPos, shootVel, ModContent.ProjectileType<WispFireBreath>(), ProjectileDamage((int)(NPC.damage * 0.5f)), 0, -1);
                }
            }
        }
        else if (AttackTimer > 150)
        {
            ResetState(ActionState.Idle);
        }
    }

    private void Enflame_CandleMash(Player player, NPC candle)
    {
        float windupEnd = 40f;
        float attackEnd = 450f;
        float restEnd = 500f;
        float slamInterval = 120f;
        SetCandleState(candle, 0, 0f);

        if (AttackTimer < attackEnd)
        {
            DoEnflameAnimation(AttackTimer, candle, false);
            if (candle.ModNPC is WispCandle wispCandle)
            {
                wispCandle.FlameState = 0;
            }
        }

        if (AttackTimer == 0 && AttackCount <= 0) 
        {
            candle.localAI[2] = 0f;
            NPC.localAI[3] = Main.rand.NextBool() ? 1f : -1f;
        }

        float candleDir = NPC.localAI[3];

        if (AttackTimer < windupEnd)
        {
            AnimateFace(-1, 5, 6, false);

            Vector2 hoverPos = player.Center - new Vector2(0, 400f + 100 * MiscHelpers.BetterEssScale(5,0.2f));
            NPC.Center = Vector2.Lerp(NPC.Center, hoverPos, 0.08f);

            Vector2 targetAim = NPC.Center - new Vector2(450 * candleDir, 0);
            candle.Center = Vector2.Lerp(candle.Center, targetAim, 0.1f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer <= attackEnd)
        {
            AnimateFace(4, 5, 6, true);

            if (AttackTimer % 120 == 0 && Main.rand.NextBool(2))
            {
                NPC.localAI[3] *= -1f;
                candleDir = NPC.localAI[3];
            }

            float speed = 1.25f;
            NPC.velocity.X = NPC.localAI[3] * speed;

            float targetY = player.Center.Y - 300f;
            NPC.velocity.Y = (targetY - NPC.Center.Y) * 0.1f;

            if (AttackTimer % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                if (HostCheck)
                {
                    int projType = ModContent.ProjectileType<WispFireRain>();
                    Vector2 topPos = NPC.Center - new Vector2(0, NPC.height / 2f);

                    for (int dir = -1; dir <= 1; dir += 2)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 shootVel = new Vector2(dir * Main.rand.NextFloat(4, 8), Main.rand.Next(-16, -10));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), topPos, shootVel, projType, ProjectileDamage((int)(NPC.damage * 0.5f)), 0, Main.myPlayer);
                        }
                    }
                }
            }

            float cycleTimer = (AttackTimer - windupEnd) % slamInterval;

            if (cycleTimer == 0)
            {
                candle.localAI[2] = 0f;
            }

            if (cycleTimer < 60f)
            {
                Vector2 targetAim = NPC.Center - new Vector2(450 * candleDir, 0);
                candle.Center = Vector2.Lerp(candle.Center, targetAim, 0.1f);
                candle.velocity = Vector2.Zero;
            }
            else
            {
                if (candle.localAI[2] == 0f)
                {
                    candle.velocity = new Vector2(0, 25f);
                    candle.netUpdate = true;
                    bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
                    bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

                    if (hitTile || hitFloor || cycleTimer == slamInterval - 1)
                    {
                        candle.velocity = Vector2.Zero;
                        candle.localAI[2] = 1f;
                        SoundEngine.PlaySound(SoundID.Item14, candle.Center);

                        if (HostCheck)
                        {
                            int projType = ModContent.ProjectileType<WispFireOrb>();

                            for (int i = 0; i < 6; i++)
                            {
                                Vector2 shootVel = new Vector2(Main.rand.NextFloat(5f, 9f) * -candleDir, Main.rand.NextFloat(-6f, -2f));
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, projType, ProjectileDamage((int)(NPC.damage * 0.5f)), 0, -1, player.whoAmI, 0, 30f);
                            }
                        }
                        AttackCount++;
                    }
                }
                else
                {
                    candle.velocity = Vector2.Zero;
                }
            }
        }
        else if (AttackTimer >= restEnd)
        {
            ResetState(ActionState.Idle);
        }
    }

    private void Enflame_Fireblow(Player player, NPC candle)
    {
        float windupEnd = 60f;
        float swingEnd = 90f;
        float restEnd = 90f;
        
        if (AttackTimer < swingEnd)
            DoEnflameAnimation(AttackTimer, candle, AttackTimer < windupEnd);

        bool swingingRight = (AttackCount % 2 == 0);
        float arcSpan = MathHelper.Pi;

        float startAngleOffset = swingingRight ? -arcSpan / 2f : arcSpan / 2f;
        float endAngleOffset = swingingRight ? arcSpan / 2f : -arcSpan / 2f;

        int burstsPerSweep = 12;

        if (AttackTimer < windupEnd)
        {
            aimPos = player.Center;
            Vector2 aimDir = NPC.DirectionTo(aimPos);
            Vector2 startPos = NPC.Center + aimDir.RotatedBy(startAngleOffset) * 120f;

            candle.Center = Vector2.Lerp(candle.Center, startPos, 0.15f);
            candle.velocity = Vector2.Zero;
        }
        else if (AttackTimer <= swingEnd)
        {
            if (AttackTimer == windupEnd)
                NPC.netUpdate = true;

            float swingDuration = swingEnd - windupEnd;
            Vector2 lockedAim = NPC.DirectionTo(aimPos);

            float progress = (AttackTimer - windupEnd) / swingDuration;
            float smoothedProgress = MathHelper.SmoothStep(0f, 1f, progress);

            float currentAngle = MathHelper.Lerp(startAngleOffset, endAngleOffset, smoothedProgress);
            Vector2 offsetDir = lockedAim.RotatedBy(currentAngle);

            candle.Center = NPC.Center + offsetDir * 180f;
            candle.velocity = Vector2.Zero;

            SetCandleState(candle, 3, offsetDir.ToRotation() + MathHelper.PiOver2);

            float previousProgress = Math.Max(0f, AttackTimer - windupEnd - 1f) / swingDuration;

            int startIndex = (AttackTimer == windupEnd) ? 0 : (int)(previousProgress * (burstsPerSweep - 1)) + 1;
            int endIndex = (int)(progress * (burstsPerSweep - 1));

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (HostCheck)
                {
                    float exactLerp = i / (float)(burstsPerSweep - 1);
                    float exactSmoothed = MathHelper.SmoothStep(0f, 1f, exactLerp);
                    float exactAngle = MathHelper.Lerp(startAngleOffset, endAngleOffset, exactSmoothed);

                    Vector2 exactOffsetDir = lockedAim.RotatedBy(exactAngle);

                    SoundEngine.PlaySound(SoundID.Item8, candle.Center);

                    float angleOffset = MathHelper.ToRadians(20) * (swingingRight ? 1 : -1);
                    Vector2 fireDir = exactOffsetDir.RotatedBy(angleOffset);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, fireDir, ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, fireDir.ToRotation(), 0, 45f);

                    for (int j = 0; j < 8; j++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, fireDir * 0.1f, ModContent.ProjectileType<WispScythe>(), ProjectileDamage((int)(NPC.damage * 0.5f)), 0, Main.myPlayer, 0, j * 12f);
                    }
                }
            }
        }
        else if (AttackTimer >= restEnd)
        {
            AttackCount++;
            AttackTimer = 0;

            if (AttackCount >= 5)
            {
                ResetState(ActionState.Idle);
            }
        }
    }
    #endregion

    #region Drawing
    public override void FindFrame(int frameHeight)
    {
        if (++NPC.frameCounter >= 6)
        {
            NPC.frameCounter = 0;
            NPC.frame.Y = (NPC.frame.Y + frameHeight) % (Main.npcFrameCount[Type] * frameHeight);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D outline = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/MotherWisp_Outline").Value;
        Texture2D face = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/MotherWisp_Face").Value;

        int frameHeight = texture.Height / Main.npcFrameCount[Type];
        int bodyFrameCurrent = NPC.frame.Y / frameHeight;

        Rectangle frameBody = texture.Frame(1, Main.npcFrameCount[Type], 0, bodyFrameCurrent);
        Rectangle frameOutline = outline.Frame(1, 1, 0, 0);
        Rectangle frameFace = face.Frame(1, faceFrameTotal, 0, faceFrameCurrent);

        Texture2D glowOrb = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle glowOrbFrame = glowOrb.Frame(1, 1, 0, 0);

        void DrawAtNPC(Texture2D tex, Rectangle rect, float scale)
        {
            sb.Draw(tex, NPC.Center + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, rect, Color.White * NPC.Opacity, NPC.rotation,
                rect.Size() / 2f, scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }

        Texture2D handTex = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/MotherWisp_Hand").Value;
        Texture2D handOutlineTex = ModContent.Request<Texture2D>("ITD/Content/NPCs/Bosses/MotherWisp_Hand_Outline").Value;

        Rectangle handFrame = handTex.Frame(1, 1, 0, 0);
        Rectangle handOutlineFrame = handOutlineTex.Frame(1, 1, 0, 0);

        SpriteEffects handEffects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        float handRotation = NPC.rotation;

        if (CandleIndex >= 0 && CandleIndex < Main.maxNPCs && Main.npc[CandleIndex].active)
        {
            NPC candle = Main.npc[CandleIndex];
            handRotation = candle.rotation;
            handEffects = candle.spriteDirection != -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        void DrawAtHand(Texture2D drawTex, Rectangle rect, float scale)
        {
            sb.Draw(drawTex, actualHandPos + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition, rect, Color.White * NPC.Opacity, handRotation,
                rect.Size() / 2f, scale, handEffects, 0f);
        }

        handEmitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () =>
        {
            if (NPC.Opacity <= 0f) return;

            if ((Vector2.DistanceSquared(handOldPos[0], handOldPos[1]) > 0.5f || AI_State == (float)ActionState.Spawning))
            {
                for (int i = 1; i < handOldPos.Length; i++)
                {
                    if (handOldPos[i] == Vector2.Zero) continue;

                    Vector2 oldHandDrawPos = handOldPos[i] - Main.screenPosition;
                    Color trailColor = new Color(131, 255, 236, 80) * NPC.Opacity * ((handOldPos.Length - i) / (float)handOldPos.Length);

                    sb.Draw(handTex, oldHandDrawPos, handFrame, trailColor, handRotation, handFrame.Size() / 2f, NPC.scale, handEffects, 0f);
                }
            }

            Main.EntitySpriteDraw(glowOrb, actualHandPos + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition, glowOrbFrame, new Color(131, 255, 236, 150) * NPC.Opacity, handRotation, glowOrbFrame.Size() / 2f, NPC.scale * 0.65f * MiscHelpers.BetterEssScale(2, 0.05f), SpriteEffects.None, 0f);
            DrawAtHand(handOutlineTex, handOutlineFrame, NPC.scale);
        });

        handEmitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () =>
        {
            if (NPC.Opacity > 0f) DrawAtHand(handTex, handFrame, NPC.scale);
        });

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => Main.EntitySpriteDraw(glowOrb, NPC.Center + Main.rand.NextVector2Circular(1f, 1f) - Main.screenPosition, glowOrbFrame, new Color(131, 255, 236, 150) * NPC.Opacity, NPC.rotation, glowOrbFrame.Size() / 2f, NPC.scale * 2f * MiscHelpers.BetterEssScale(2, 0.05f), SpriteEffects.None, 0f));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline, frameOutline, NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => Main.EntitySpriteDraw(face, NPC.Center + new Vector2(0, 0 * NPC.scale) - Main.screenPosition, frameFace, Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() / 2f, NPC.scale, SpriteEffects.None));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () => DrawAtNPC(texture, frameBody, NPC.scale));

        return false;
    }
    #endregion
}