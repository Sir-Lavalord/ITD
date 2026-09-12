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

    public ParticleEmitter emitter;
    public ParticleEmitter handEmitter;

    private enum ActionState
    {
        Spawning,
        Idle,
        ChooseCombo,
        ExecuteCombo,
        Die,
        Stunned
    }
    /// <summary>
    /// <para> This is the base attack of mWisp </para>
    /// <para>Boss picks from these attacks and then picks a secondary attack to combo with it </para>
    /// <para>Boss does not pick the same main attack and secondary attack </para>
    /// <para>Boss does not pick the same main attack twice in a row, but it can pick the same secondary attack twice in a row </para>
    /// <para>Boss does not pick the same main attack thrice in a row, regardless of the secondary </para>
    /// </summary>
    public enum BaseAttack
    {
        None = -1,
        CandleMash = 0,
        Fireblow = 1,
        Enflame = 2,
        Split = 3
    }

    public ref float AI_State => ref NPC.ai[1];
    public ref float MainAttack => ref NPC.ai[2];
    public ref float SecAttack => ref NPC.ai[3];

    public ref float AttackTimer => ref NPC.localAI[0];
    public ref float AttackCount => ref NPC.localAI[1];

    public int maxWispCount = 10;
    public float GrandWispsLost
    {
        get => NPC.localAI[2];
        set => NPC.localAI[2] = value;
    }

    public Vector2 aimPos;

    public int CandleIndex => (int)NPC.ai[0];
    public bool HostCheck => Main.netMode != NetmodeID.MultiplayerClient;

    int faceFrameTotal = 6;
    int faceFrameCurrent = 0;
    int faceFrameCounter = 0; //epic, took too long to figure out
    private int consecutiveMainCount = 0;

    public Vector2 actualHandPos;
    public Vector2[] handOldPos = new Vector2[12]; // 12 is too much already

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
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit37;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.boss = true;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;

        handEmitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
        handEmitter.tag = NPC;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance * bossAdjustment);
        NPC.damage = (int)(NPC.damage * 0.7f);
    }

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
    /// <summary>
    /// <para> This is used to animate face only </para>
    /// <para>Frame start and frame end are inclusive, so if you want to animate frames 0, 1, 2, you would call AnimateFace(0, 2, speed)</para>
    /// <para>Frame start =-1 makes it starts from current frame</para>
    /// <para>doLoop determines if the animation should loop or not</para>
    /// </summary>
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

    private void ApplyFriction()
    {
        NPC.velocity *= 0.8f;
        if (NPC.velocity.LengthSquared() < 0.01f)
            NPC.velocity = Vector2.Zero;
    }

    private void SetCandleState(NPC candle, int state, float angle = 0f)
    {
        candle.localAI[0] = state;
        candle.localAI[1] = angle;
    }

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

        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();

        Player player = Main.player[NPC.target];

        if (MainAttack != (float)BaseAttack.Split)
        {
            NPC.Opacity = 1f;
            NPC.dontTakeDamage = false;
            NPC.ShowNameOnHover = true;
        }

        int particleCount = Main.rand.Next(4, 7);
        for (int i = 0; i < particleCount; i++)
        {
            if (NPC.Opacity <= 0f) continue;

            float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 2.5f;
            Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(8f, 10.5f) * NPC.scale);
            Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.2f, NPC.height / 2.2f) * NPC.scale;
            emitter?.Emit(NPC.Center + spawnOffset, mistVelocity, 0f);
        }

        switch ((ActionState)AI_State)
        {
            case ActionState.Spawning:
                AnimateFace(0, 5, 6);
                float morphTime = 60;
                float reachTime = 60;
                float progress = Utils.Clamp(AttackTimer / morphTime, 0f, 1f);

                NPC.scale = MathHelper.Lerp(0f, 1.5f, progress);

                Vector2 hoverPos = new Vector2(candle.Center.X, candle.Top.Y - (120f * NPC.scale));
                NPC.Center = Vector2.Lerp(NPC.Center, hoverPos, 0.1f);
                ApplyFriction();

                if (candle.ModNPC is WispCandle wispCandleSpawn)
                {
                    if (AttackTimer < morphTime)
                    {
                        actualHandPos = NPC.Center + new Vector2(NPC.spriteDirection * 55f, 10f) * NPC.scale;
                    }
                    else
                    {
                        Vector2 targetHandle = candle.Center + new Vector2(wispCandleSpawn.currentHandX, 8f) * candle.scale;
                        actualHandPos = Vector2.Lerp(actualHandPos, targetHandle, 0.12f);
                    }
                }

                if (AttackTimer++ >= morphTime + reachTime)
                {
                    ResetState(ActionState.Idle);
                }
                break;

            case ActionState.Idle:
                AnimateFace(0, 0, 6);
                AttackTimer++;
                GeneralHover(player, 300f);
                CandleIdleHover(candle);

                if (AttackTimer >= 180)
                {
                    ResetState(ActionState.ChooseCombo);
                }
                break;

            case ActionState.Stunned:
                AnimateFace(3, 5, 6);
                GeneralHover(player, 300f);
                CandleIdleHover(candle);

                float stunDuration = 120f;
                if (AttackTimer++ >= stunDuration)
                {
                    ResetState(ActionState.Idle);
                }
                break;

            case ActionState.ChooseCombo:
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

                    if (MainAttack == prevMain && SecAttack == prevSec)
                    {
                        SecAttack = (SecAttack + 1) % 3;
                        if (SecAttack == MainAttack) SecAttack = (SecAttack + 1) % 3;
                    }

                    if (MainAttack == prevMain) consecutiveMainCount++;
                    else consecutiveMainCount = 1;

                    AI_State = (float)ActionState.ExecuteCombo;
                    NPC.netUpdate = true;
                }
                break;

            case ActionState.ExecuteCombo:
                switch ((BaseAttack)MainAttack)
                {
                    case BaseAttack.CandleMash: CandleMash(player, candle, (BaseAttack)SecAttack); break;
                    case BaseAttack.Fireblow: Fireblow(player, candle, (BaseAttack)SecAttack); break;
                    case BaseAttack.Enflame: Enflame(player, candle, (BaseAttack)SecAttack); break;
                    case BaseAttack.Split: SplitAttack(player, candle); break;
                }
                break;
        }

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

        float targetBossRotation = 0f;
        if (AI_State == (float)ActionState.Idle || AI_State == (float)ActionState.Spawning || AI_State == (float)ActionState.Stunned)
        {
            float maxRotation = MathHelper.Pi / 6;
            float rotationFactor = MathHelper.Clamp(NPC.velocity.X / 8f, -1f, 1f);
            targetBossRotation = rotationFactor * maxRotation;
        }
        NPC.rotation = Utils.AngleLerp(NPC.rotation, targetBossRotation, 0.15f);
    }

    private void SplitAttack(Player player, NPC candle)
    {
        AttackTimer++;
        float time = AttackTimer;
        float shakeEnd = 120f;
        float danceTime = 420f;

        candle.velocity *= 0.8f;
        if (candle.velocity.Length() < 0.1f) candle.velocity = Vector2.Zero;

        if (time < shakeEnd)
        {
            if (time <= 90)
            {
                AnimateFace(3, 5, 6, true);
            }
            else
            {
                AnimateFace(-1, 5, 6, false);
            }

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

            if (time == shakeEnd)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

                GrandWispsLost = Math.Max(GrandWispsLost, 5);
                int currentPool = maxWispCount - (int)GrandWispsLost;
                int countToSpawn = Math.Max(5, currentPool);

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

            bool forceReform = (time - shakeEnd) >= danceTime;
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
                if (forceReform || time > shakeEnd + 5f)
                {
                    NPC.Opacity = 1f;

                    if (GrandWispsLost >= maxWispCount)
                    {
                        if (HostCheck)
                        {
                            NPC.StrikeInstantKill();
                        }
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

    private void DoEnflameAnimation(float AttackTimer, NPC candle, bool isWindup)
    {
        int extraParticles = (int)MathHelper.Min(AttackTimer / 3f, 12f);

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

    private void GeneralHover(Player player, float hoverHeight = 300f, float speed = 0.05f)
    {
        float verticalBob = MiscHelpers.BetterEssScale(2, 0.2f);
        Vector2 hoverTarget = player.Center - new Vector2(0, hoverHeight * verticalBob);
        NPC.velocity = (hoverTarget - NPC.Center) * speed;
    }

    private void CandleIdleHover(NPC candle)
    {
        Vector2 targetPos = NPC.Center + new Vector2(0, 160f);
        candle.velocity = (targetPos - candle.Center) * 0.1f;
    }

    private void CandleMash(Player player, NPC candle, BaseAttack sec)
    {
        AnimateFace(0, 5, 6);
        AttackTimer++;
        float time = AttackTimer;
        ApplyFriction();

        if (sec == BaseAttack.None || sec == BaseAttack.Fireblow)
        {
            float windupEnd = 40f;
            float positionEnd = 60f;
            float attackTimeout = 120f;
            float restEnd = 150f;

            if (time < windupEnd)
            {
                Vector2 handPos = NPC.Center + new Vector2(NPC.direction * 180, 50);
                candle.Center = Vector2.Lerp(candle.Center, handPos, 0.15f);
                candle.velocity = Vector2.Zero;
            }
            else if (time < positionEnd)
            {
                Vector2 targetAim = player.Center - new Vector2(0, 250);
                candle.Center = Vector2.Lerp(candle.Center, targetAim, 0.2f);
                candle.velocity = Vector2.Zero;
            }
            else if (time == positionEnd)
            {
                candle.velocity = new Vector2(0, 25f);
                candle.netUpdate = true;
            }
            else if (time > positionEnd && time <= attackTimeout)
            {
                bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
                bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

                if (hitTile || hitFloor || time == attackTimeout)
                {
                    candle.velocity = Vector2.Zero;
                    SoundEngine.PlaySound(SoundID.Item14, candle.Center);

                    if (HostCheck)
                    {
                        for (int j = -1; j <= 1; j += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Bottom + new Vector2(30 * j, -20), new Vector2(8 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), (int)(NPC.damage * 0.5f), 0, -1);
                        }
                    }

                    AttackTimer = attackTimeout;
                }
            }
            else if (time > attackTimeout && time < restEnd)
            {
                candle.velocity = Vector2.Zero;
            }
            else if (time >= restEnd)
            {
                AttackCount++;
                if (AttackCount >= 3) ResetState(ActionState.Idle);
                else AttackTimer = 0;
            }
        }
        else if (sec == BaseAttack.Enflame)
        {
            float windupEnd = 60f;
            float positionEnd = 90f;
            float attackTimeout = 180f;
            float restEnd = 210f;

            if (candle.ModNPC is WispCandle wispCandle)
                wispCandle.FlameState = 1;

            if (time < windupEnd)
            {
                Vector2 handPos = NPC.Center + new Vector2(NPC.direction * 180, 50);
                candle.Center = Vector2.Lerp(candle.Center, handPos, 0.1f);
                candle.velocity = Vector2.Zero;
            }
            else if (time < positionEnd)
            {
                Vector2 targetAim = player.Center - new Vector2(0, 250);
                candle.Center = Vector2.Lerp(candle.Center, targetAim, 0.1f);
                candle.velocity = Vector2.Zero;
            }
            else if (time == positionEnd)
            {
                candle.velocity = new Vector2(0, 20f);
                candle.netUpdate = true;
            }
            else if (time > positionEnd && time <= attackTimeout)
            {
                bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
                bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

                if (hitTile || hitFloor || time == attackTimeout)
                {
                    candle.velocity = Vector2.Zero;
                    SoundEngine.PlaySound(SoundID.Item14, candle.Center);

                    if (HostCheck)
                    {
                        for (int j = -1; j <= 1; j += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Bottom + new Vector2(30 * j, -20), new Vector2(6 * j, 0), ModContent.ProjectileType<CosmicShockwave>(), (int)(NPC.damage * 0.75f), 0, -1);
                        }
                    }

                    AttackTimer = attackTimeout;
                }
            }
            else if (time > attackTimeout && time < restEnd)
            {
                candle.velocity = Vector2.Zero;
            }
            else if (time >= restEnd)
            {
                AttackCount++;
                if (AttackCount >= 3) ResetState(ActionState.Idle);
                else AttackTimer = 0;
            }
        }
    }

    private void Fireblow(Player player, NPC candle, BaseAttack sec)
    {
        AnimateFace(0, 5, 6);
        AttackTimer++;
        float time = AttackTimer;

        if (time < 40f)
        {
            float hoverHeight = sec == BaseAttack.Enflame ? 600 : 300;
            GeneralHover(player, hoverHeight);
        }
        else
        {
            ApplyFriction();
        }

        if (sec == BaseAttack.None)
        {
            float windupEnd = 40f;
            float blowEnd = 80f;
            float resetTime = 150f;

            if (time < windupEnd) aimPos = player.Center;
            Vector2 aimDir = NPC.DirectionTo(aimPos);

            if (time < windupEnd)
            {
                Vector2 targetPos = NPC.Center + aimDir * 50f;
                candle.Center = Vector2.Lerp(candle.Center, targetPos, 0.2f);
                candle.velocity = Vector2.Zero;
            }
            else if (time == windupEnd)
            {
                NPC.netUpdate = true;
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, Vector2.Zero, ModContent.ProjectileType<WispFireBreathTelegraph>(), 0, 0, Main.myPlayer, aimDir.ToRotation(), candle.whoAmI);
                }
            }
            else if (time > windupEnd && time <= blowEnd)
            {
                Vector2 targetPos = NPC.Center + aimDir * 100f;
                candle.Center = targetPos;
                candle.velocity = Vector2.Zero;

                if (time % 2 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                    if (HostCheck)
                    {
                        float spread = MathHelper.ToRadians(25);
                        Vector2 shootVel = aimDir.RotatedByRandom(spread) * Main.rand.NextFloat(8f, 12f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, ModContent.ProjectileType<WispFireBreath>(), (int)(NPC.damage * 0.5f), 0, -1);
                    }
                }
            }
            else if (time >= resetTime)
            {
                ResetState(ActionState.Idle);
            }
        }
        else if (sec == BaseAttack.CandleMash)
        {
            float windupEnd = 40f;
            float blowEnd = 80f;
            float positionEnd = 160f;
            float attackTimeout = 220f;
            float telegraphStart = 240f;
            float blastTime = 270f;
            float restEnd = 300f;

            if (time < windupEnd) aimPos = player.Center;
            Vector2 aimDir = NPC.DirectionTo(aimPos);

            if (time < windupEnd)
            {
                Vector2 targetPos = NPC.Center + aimDir * 50f;
                candle.Center = Vector2.Lerp(candle.Center, targetPos, 0.2f);
                candle.velocity = Vector2.Zero;
            }
            else if (time == windupEnd)
            {
                NPC.netUpdate = true;
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, Vector2.Zero, ModContent.ProjectileType<WispFireBreathTelegraph>(), 0, 0, Main.myPlayer, aimDir.ToRotation(), candle.whoAmI);
                }
            }
            else if (time > windupEnd && time <= blowEnd)
            {
                Vector2 targetPos = NPC.Center + aimDir * 100f;
                candle.Center = targetPos;
                candle.velocity = Vector2.Zero;

                if (time % 2 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item34, candle.Center);
                    if (HostCheck)
                    {
                        float spread = MathHelper.ToRadians(25);
                        Vector2 shootVel = aimDir.RotatedByRandom(spread) * Main.rand.NextFloat(8f, 12f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, ModContent.ProjectileType<WispFireBreath>(), (int)(NPC.damage * 0.5f), 0, -1);
                    }
                }
            }
            else if (time > blowEnd)
            {
                if (time < positionEnd)
                {
                    Lighting.AddLight(candle.Center, 0.8f, 0.4f, 0f);
                    if (candle.ModNPC is WispCandle wispCandle) wispCandle.FlameState = 2;

                    Vector2 aimPosSmash = player.Center - new Vector2(0, 300f);
                    candle.Center = Vector2.Lerp(candle.Center, aimPosSmash, 0.08f);
                    candle.velocity = Vector2.Zero;
                }
                else if (time == positionEnd)
                {
                    candle.velocity = new Vector2(0, 35f);
                    candle.netUpdate = true;
                }
                else if (time > positionEnd && time <= attackTimeout)
                {
                    bool hitTile = Collision.SolidCollision(candle.position, candle.width, candle.height);
                    bool hitFloor = candle.Bottom.Y >= player.Bottom.Y;

                    if (hitTile || hitFloor || time == attackTimeout)
                    {
                        candle.velocity = Vector2.Zero;
                        SoundEngine.PlaySound(SoundID.Item14, candle.Center);
                        aimPos = player.Center;
                        NPC.netUpdate = true;

                        if (HostCheck)
                        {
                            Vector2 baseDirection = NPC.SafeDirectionTo(aimPos);
                            for (int i = 0; i < 4; i++)
                            {
                                Vector2 offset = baseDirection.RotatedBy(Math.PI * 2 / 4 * i);
                                Projectile.NewProjectile(candle.GetSource_FromThis(), candle.Center, offset, ModContent.ProjectileType<WispTelegraph>(), 0, 0f, Main.myPlayer, 0f, 0f, 30f);
                            }
                        }

                        AttackTimer = telegraphStart;
                    }
                }
                else if (time > telegraphStart && time < blastTime)
                {
                    candle.velocity = Vector2.Zero;
                }
                else if (time == blastTime)
                {
                    candle.velocity = Vector2.Zero;

                    if (HostCheck)
                    {
                        Vector2 baseDirection = NPC.SafeDirectionTo(aimPos);
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 offset = NPC.height / 2 * baseDirection.RotatedBy(Math.PI * 2 / 4 * i);
                            float ai1 = i <= 1 || i == 3 ? 32 : 8;
                            Projectile.NewProjectile(candle.GetSource_FromThis(), candle.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2), Vector2.Zero, ModContent.ProjectileType<WispChainBlast>(), NPC.defDamage, 0f, Main.myPlayer, MathHelper.WrapAngle(offset.ToRotation()), ai1);
                        }
                    }
                }
                else if (time > blastTime && time < restEnd)
                {
                    candle.velocity = Vector2.Zero;
                }
                else if (time >= restEnd)
                {
                    ResetState(ActionState.Idle);
                }
            }
        }
        else if (sec == BaseAttack.Enflame)
        {
            float windupEnd = 60f;
            float blowEnd = 300f;
            float restEnd = 350f;
            float spread = MathHelper.ToRadians(25);

            if (time < blowEnd) DoEnflameAnimation(time, candle, time < windupEnd);

            if (time < windupEnd)
            {
                aimPos = player.Center;
                Vector2 aimDir = NPC.DirectionTo(aimPos);
                Vector2 targetPos = NPC.Center + aimDir * 50f;

                candle.Center = Vector2.Lerp(candle.Center, targetPos, 0.2f);
                candle.velocity = Vector2.Zero;
            }
            else if (time == windupEnd)
            {
                NPC.netUpdate = true;
                Vector2 lockedAim = NPC.DirectionTo(aimPos);

                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, Vector2.Zero, ModContent.ProjectileType<WispFireBreathTelegraph>(), 0, 0, Main.myPlayer, lockedAim.ToRotation(), candle.whoAmI);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, lockedAim.RotatedBy(-spread), ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, 0f, 0f, 120f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, lockedAim.RotatedBy(spread), ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, 0f, 0f, 120f);
                }
            }
            else if (time > windupEnd && time <= blowEnd)
            {
                aimPos = Vector2.Lerp(aimPos, player.Center, 0.025f);
                Vector2 currentAim = NPC.DirectionTo(aimPos);

                Vector2 targetPos = NPC.Center + currentAim * 100f;
                candle.Center = targetPos;
                candle.velocity = Vector2.Zero;

                int projType = ModContent.ProjectileType<WispFireBreath>();

                if (time % 40 == 0)
                {
                    AttackCount++;
                    int numProjectiles = AttackCount % 2 == 0 ? 5 : 6;
                    float rotation = MathHelper.ToRadians(30);
                    Vector2 baseVelocity = currentAim * 14f;

                    if (HostCheck)
                    {
                        for (int i = 0; i < numProjectiles; i++)
                        {
                            float currentRotation = MathHelper.Lerp(-rotation, rotation, i / (float)(numProjectiles - 1));
                            Vector2 shootVel = baseVelocity.RotatedBy(currentRotation);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, projType, NPC.damage, 1f, Main.myPlayer);
                        }
                    }
                }

                if (time % 2 == 0)
                {
                    if (time % 6 == 0) SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                    if (HostCheck)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, currentAim.RotatedBy(-spread) * 16f, projType, (int)(NPC.damage * 0.5f), 0, -1);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, currentAim.RotatedBy(spread) * 16f, projType, (int)(NPC.damage * 0.5f), 0, -1);
                    }
                }
            }
            else if (time > blowEnd && time < restEnd)
            {
                candle.velocity = Vector2.Zero;
            }
            else if (time >= restEnd)
            {
                ResetState(ActionState.Idle);
            }
        }
    }

    private void Enflame(Player player, NPC candle, BaseAttack sec)
    {
        AnimateFace(0, 4, 6);
        AttackTimer++;
        float time = AttackTimer;

        CandleIdleHover(candle);
        ApplyFriction();

        if (sec == BaseAttack.None)
        {
            if (time < 120) DoEnflameAnimation(time, candle, true);

            if (time > 40 && time < 120)
            {
                if (time % 6 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item20, candle.Center);

                    if (HostCheck)
                    {
                        Vector2 tipPos = candle.Top - new Vector2(0, 20f);
                        Vector2 shootVel = new Vector2(Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-14f, -9f));
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), tipPos, shootVel, ModContent.ProjectileType<WispFireBreath>(), (int)(NPC.damage * 0.5f), 0, -1);
                    }
                }
            }
            else if (time > 150)
            {
                ResetState(ActionState.Idle);
            }
        }
        else if (sec == BaseAttack.CandleMash)
        {
            float windupEnd = 60f;
            float swingEnd = 120f;
            float restEnd = 160f;

            if (time < swingEnd) DoEnflameAnimation(time, candle, time < windupEnd);

            if (time < windupEnd)
            {
                aimPos = player.Center;
                Vector2 aimDir = NPC.DirectionTo(aimPos);

                Vector2 startPos = NPC.Center + aimDir.RotatedBy(-MathHelper.PiOver2) * 120f;
                candle.Center = Vector2.Lerp(candle.Center, startPos, 0.15f);
                candle.velocity = Vector2.Zero;
            }
            else if (time <= swingEnd)
            {
                if (time == windupEnd) NPC.netUpdate = true;

                Vector2 lockedAim = NPC.DirectionTo(aimPos);
                float progress = (time - windupEnd) / (swingEnd - windupEnd);
                float smoothedProgress = MathHelper.SmoothStep(0f, 1f, progress);

                float currentAngle = MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, smoothedProgress);
                Vector2 offsetDir = lockedAim.RotatedBy(currentAngle);

                candle.Center = NPC.Center + offsetDir * 180f;
                candle.velocity = Vector2.Zero;

                SetCandleState(candle, 3, offsetDir.ToRotation() + MathHelper.PiOver2);

                if (time % 4 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                    if (HostCheck)
                    {
                        int projType = ModContent.ProjectileType<WispFireBreath>();
                        float spread = MathHelper.ToRadians(15);

                        for (int i = -1; i <= 1; i++)
                        {
                            Vector2 shootVel = offsetDir.RotatedBy(spread * i) * Main.rand.NextFloat(10f, 16f);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, projType, (int)(NPC.damage * 0.5f), 0, -1);
                        }
                    }
                }
            }
            else if (time >= restEnd)
            {
                ResetState(ActionState.Idle);
            }
        }
        else if (sec == BaseAttack.Fireblow)
        {
            float windupEnd = 60f;
            float blowEnd = 160f;
            float restEnd = 200f;

            if (time < blowEnd) DoEnflameAnimation(time, candle, time < windupEnd);

            if (time > windupEnd && time <= blowEnd)
            {
                if (time % 5 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                    float sweepAngle = (float)Math.Sin((time - windupEnd) * 0.25f) * 0.8f;
                    float baseAngle = -MathHelper.PiOver2 + sweepAngle;

                    if (HostCheck)
                    {
                        Vector2 tipPos = candle.Top - new Vector2(0, 20f);
                        int projType = ModContent.ProjectileType<WispFireBreath>();

                        for (int i = -1; i <= 1; i++)
                        {
                            Vector2 shootVel = (baseAngle + (i * 0.2f)).ToRotationVector2() * Main.rand.NextFloat(12f, 18f);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), tipPos, shootVel, projType, (int)(NPC.damage * 0.5f), 0, -1);
                        }
                    }
                }
            }
            else if (time >= restEnd)
            {
                ResetState(ActionState.Idle);
            }
        }
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

        AI_State = (float)nextState;
        AttackTimer = 0;
        AttackCount = 0;
        MainAttack = -1;
        SecAttack = -1;
        NPC.netUpdate = true;
    }

    public override void FindFrame(int frameHeight)
    {
        if (++NPC.frameCounter >= 6) // body only
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

        Rectangle frameBody = texture.Frame(1, Main.npcFrameCount[Type], 0, bodyFrameCurrent); // split now
        Rectangle frameOutline = outline.Frame(1, 1, 0, 0);
        Rectangle frameFace = face.Frame(1, faceFrameTotal, 0, faceFrameCurrent); // fixed sht

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
}