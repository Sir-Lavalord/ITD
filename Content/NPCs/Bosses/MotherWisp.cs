using ITD.Content.Projectiles.Hostile.CosJel;
using ITD.Content.Projectiles.Hostile.MotherWisp;
using ITD.Particles;
using ITD.Particles.Misc;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Particles;
using ITD.Particles.Projectiles;
using ITD.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
namespace ITD.Content.NPCs.Bosses;

[AutoloadBossHead]
public class MotherWisp : ModNPC
{
    public ParticleEmitter emitter;

    private enum ActionState
    {
        Spawning,
        Idle,
        ChooseCombo,
        ExecuteCombo,
        Die
    }

    public enum BaseAttack
    {
        None = -1,
        CandleMash = 0,
        Fireblow = 1,
        Enflame = 2
    }

    public ref float AI_State => ref NPC.ai[1];
    public ref float MainAttack => ref NPC.ai[2];
    public ref float SecAttack => ref NPC.ai[3];

    public ref float AttackTimer => ref NPC.localAI[0];
    public ref float AttackCount => ref NPC.localAI[1];

    public Vector2 aimPos;

    public int CandleIndex => (int)NPC.ai[0];

    int faceFrameTotal = 7;
    int faceFrameCurrent = 0;
    private int consecutiveMainCount = 0;

    public override void SetStaticDefaults()
    {
    }

    public override void SetDefaults()
    {
        NPC.width = 100;
        NPC.height = 100;
        NPC.damage = 30;
        NPC.defense = 0;
        NPC.lifeMax = 1000;
        NPC.HitSound = SoundID.NPCHit42;
        NPC.DeathSound = SoundID.NPCDeath44;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.boss = true;
        emitter = ParticleSystem.NewEmitter<WispMist>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
        emitter.tag = NPC;
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
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        AttackTimer = reader.ReadSingle();
        AttackCount = reader.ReadSingle();
        aimPos.X = reader.ReadSingle();
        aimPos.Y = reader.ReadSingle();
        consecutiveMainCount = reader.ReadInt32();
    }

    public override void AI()
    {
        NPC candle = MiscHelpers.NPCExists(CandleIndex, ModContent.NPCType<WispCandle>());
        if (candle == null)
        {
            NPC.active = false;
            return;
        }

        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            NPC.TargetClosest();

        Player player = Main.player[NPC.target];

        if (emitter != null) emitter.keptAlive = true;

        int particleCount = Main.rand.Next(4, 7);
        for (int i = 0; i < particleCount; i++)
        {
            float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 12f)) * 2.5f;
            Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(8f, 10.5f) * NPC.scale);
            Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.2f, NPC.height / 2.2f) * NPC.scale;
            emitter?.Emit(NPC.Center + spawnOffset, mistVelocity, 0f);
        }

        switch ((ActionState)AI_State)
        {
            case ActionState.Spawning:
                AttackTimer++;
                NPC.scale = MathHelper.Lerp(0f, 1.5f, AttackTimer / 60f);
                NPC.Center = Vector2.Lerp(NPC.Center, candle.Center - new Vector2(0, 90), 0.1f);
                if (AttackTimer >= 60)
                {
                    ResetState(ActionState.Idle);
                }
                break;

            case ActionState.Idle:
                AttackTimer++;
                GeneralHover(player, 300f);
                CandleIdleHover(candle);

                if (AttackTimer >= 180)
                {
                    ResetState(ActionState.ChooseCombo);
                }
                break;

            case ActionState.ChooseCombo:
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float prevMain = MainAttack;
                    float prevSec = SecAttack;

                    MainAttack = Main.rand.Next(0,2);

                    if (MainAttack == prevMain && consecutiveMainCount >= 2)
                    {
                        MainAttack = (MainAttack + Main.rand.Next(1, 3)) % 3;
                    }

                    SecAttack = Main.rand.Next(0, 3);

                    if (SecAttack == MainAttack)
                    {
                        SecAttack = (MainAttack + 1) % 3;
                    }

                    if (MainAttack == prevMain && SecAttack == prevSec)
                    {
                        SecAttack = (SecAttack + 1) % 3;

                        if (SecAttack == MainAttack)
                        {
                            SecAttack = (SecAttack + 1) % 3;
                        }
                    }

                    if (MainAttack == prevMain)
                    {
                        consecutiveMainCount++;
                    }
                    else
                    {
                        consecutiveMainCount = 1;
                    }

                    AI_State = (float)ActionState.ExecuteCombo;
                    NPC.netUpdate = true;
                }
                break;

            case ActionState.ExecuteCombo:
                switch ((BaseAttack)MainAttack)
                {
                    case BaseAttack.CandleMash:
                        CandleMash(player, candle, (BaseAttack)SecAttack);
                        break;
                    case BaseAttack.Fireblow:
                        Fireblow(player, candle, (BaseAttack)SecAttack);
                        break;
                    case BaseAttack.Enflame:
                        Enflame(player, candle, (BaseAttack)SecAttack);
                        break;
                }
                break;

            case ActionState.Die:
                break;
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
        AttackTimer++;
        float time = AttackTimer;
        NPC.velocity *= 0.8f;
        if (NPC.velocity.Length() < 0.1f) NPC.velocity = Vector2.Zero;

        if (sec == BaseAttack.None)
        {
            
        }
        else if (sec == BaseAttack.Fireblow)
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

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int j = -1; j <= 1; j += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Bottom + new Vector2(30 * j, -20), new Vector2(8 * j, 0),
                                ModContent.ProjectileType<CosmicShockwave>(), (int)(NPC.damage * 0.5f), 0, -1);
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
                AttackTimer = 0;

                if (AttackCount >= 3)
                {
                    ResetState(ActionState.Idle);
                }
            }
        }
        else if (sec == BaseAttack.Enflame)
        {
            float windupEnd = 60f;
            float positionEnd = 90f;
            float attackTimeout = 180f;
            float restEnd = 210f;

            if (candle.ModNPC is WispCandle wispCandle)
            {
                wispCandle.FlameState = 1;
            }

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

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int j = -1; j <= 1; j += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Bottom + new Vector2(30 * j, -20), new Vector2(6 * j, 0),
                                ModContent.ProjectileType<CosmicShockwave>(), (int)(NPC.damage * 0.75f), 0, -1);
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
                AttackTimer = 0;

                if (AttackCount >= 3)
                {
                    ResetState(ActionState.Idle);
                }
            }
        }
    }

    private void Fireblow(Player player, NPC candle, BaseAttack sec)
    {
        AttackTimer++;
        float time = AttackTimer;


        if (time < 40f)
        {
            float hoverHeight = sec == BaseAttack.Enflame ? 600 : 300;
            GeneralHover(player, hoverHeight);
        }
        else
        {
            NPC.velocity *= 0.8f;
            if (NPC.velocity.Length() < 0.1f) NPC.velocity = Vector2.Zero;
        }
    

        if (sec == BaseAttack.None)
        {
            float windupEnd = 40f;
            float blowEnd = 80f;
            float resetTime = 150f;

            if (time < windupEnd)
            {
                aimPos = player.Center;
            }
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
                if (Main.netMode != NetmodeID.MultiplayerClient)
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

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float spread = MathHelper.ToRadians(25);
                        Vector2 shootVel = aimDir.RotatedByRandom(spread) * Main.rand.NextFloat(8f, 12f);
                        int projType = ModContent.ProjectileType<WispFireBreath>();

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, projType, (int)(NPC.damage * 0.5f), 0, -1);
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

            if (time < windupEnd)
            {
                aimPos = player.Center;
            }
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
                if (Main.netMode != NetmodeID.MultiplayerClient)
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

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float spread = MathHelper.ToRadians(25);
                        Vector2 shootVel = aimDir.RotatedByRandom(spread) * Main.rand.NextFloat(8f, 12f);
                        int projType = ModContent.ProjectileType<WispFireBreath>();

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel, projType, (int)(NPC.damage * 0.5f), 0, -1);
                    }
                }
            }
            else if (time > blowEnd)
            {
                if (time < positionEnd)
                {
                    Lighting.AddLight(candle.Center, 0.8f, 0.4f, 0f);

                    if (candle.ModNPC is WispCandle wispCandle)
                    {
                        wispCandle.FlameState = 2;
                    }

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

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 baseDirection = NPC.SafeDirectionTo(aimPos);
                            const int max = 4;
                            for (int i = 0; i < max; i++)
                            {
                                Vector2 offset = baseDirection.RotatedBy(Math.PI * 2 / max * i);

                                Projectile.NewProjectile(candle.GetSource_FromThis(), candle.Center, offset, ModContent.ProjectileType<WispTelegraph>(),
                                    0, 0f, Main.myPlayer, 0f, 0f, 30f);
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

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 baseDirection = NPC.SafeDirectionTo(aimPos);
                        const int max = 4;
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 offset = NPC.height / 2 * baseDirection.RotatedBy(Math.PI * 2 / max * i);
                            float ai1 = i <= 1 || i == max - 1 ? 32 : 8;

                            Projectile.NewProjectile(candle.GetSource_FromThis(), candle.Center + Main.rand.NextVector2Circular(NPC.width / 2, NPC.height / 2), Vector2.Zero, ModContent.ProjectileType<WispChainBlast>(),
                                (NPC.defDamage), 0f, Main.myPlayer, MathHelper.WrapAngle(offset.ToRotation()), ai1);
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

            int extraParticles = (int)MathHelper.Min(time / 3f, 10f);
            for (int i = 0; i < extraParticles; i++)
            {
                float wiggle = (float)Math.Sin((Main.GlobalTimeWrappedHourly * 24f) + Main.rand.NextFloat(MathHelper.TwoPi)) * 3.5f;
                Vector2 mistVelocity = new Vector2(wiggle, -Main.rand.NextFloat(14f, 22f) * NPC.scale);
                Vector2 spawnOffset = Main.rand.NextVector2Circular(NPC.width / 2.5f, NPC.height / 2.5f) * NPC.scale;
                emitter?.Emit(NPC.Center + spawnOffset, mistVelocity, 0f);
            }

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

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, Vector2.Zero, ModContent.ProjectileType<WispFireBreathTelegraph>(), 0, 0, Main.myPlayer, lockedAim.ToRotation(), candle.whoAmI);

                    Vector2 lineVel1 = lockedAim.RotatedBy(-spread);
                    Vector2 lineVel2 = lockedAim.RotatedBy(spread);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, lineVel1, ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, 0f, 0f, 120f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, lineVel2, ModContent.ProjectileType<WispTelegraph>(), 0, 0, Main.myPlayer, 0f, 0f, 120f);
                }
            }
            else if (time > windupEnd && time <= blowEnd)
            {
                aimPos = Vector2.Lerp(aimPos, player.Center, 0.025f);
                Vector2 currentAim = NPC.DirectionTo(aimPos);

                Vector2 targetPos = NPC.Center + currentAim * 100f;
                candle.Center = targetPos;
                candle.velocity = Vector2.Zero;
                int numProjectiles = 6;
                int projType = ModContent.ProjectileType<WispFireBreath>();

                if (time % 40 == 0)
                {
                    AttackCount++;
                    numProjectiles = AttackCount % 2 == 0 ? 5: 6;
                    float rotation = MathHelper.ToRadians(30);
                    Vector2 baseVelocity = NPC.DirectionTo(aimPos) * 14f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < numProjectiles; i++)
                        {   float currentRotation = MathHelper.Lerp(-rotation, rotation, i / (float)(numProjectiles - 1));
                            Vector2 shootVel = baseVelocity.RotatedBy(currentRotation);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, projType, NPC.damage, 1f, Main.myPlayer);
                        }
                    }
                }
                if (time % 2 == 0)
                {
                    if (time % 6 == 0)
                        SoundEngine.PlaySound(SoundID.Item34, candle.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {

                        Vector2 shootVel1 = currentAim.RotatedBy(-spread) * 16f;
                        Vector2 shootVel2 = currentAim.RotatedBy(spread) * 16f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel1, projType, (int)(NPC.damage * 0.5f), 0, -1);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), candle.Center, shootVel2, projType, (int)(NPC.damage * 0.5f), 0, -1);
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
        if (sec == BaseAttack.None)
        {
            AttackTimer++;
            CandleIdleHover(candle);

            NPC.velocity *= 0.8f;
            if (NPC.velocity.Length() < 0.1f) NPC.velocity = Vector2.Zero;

            if (AttackTimer < 120)
            {
                int extraParticles = (int)MathHelper.Min(AttackTimer / 3f, 10f);

                if (candle.ModNPC is WispCandle wispCandle)
                {
                    wispCandle.FlameState = 1;
                    wispCandle.ExtraParticles = extraParticles;
                }
            }

            if (AttackTimer > 40 && AttackTimer < 120)
            {
                if (AttackTimer % 6 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item20, candle.Center);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 tipPos = candle.Top - new Vector2(0, 20f);
                        Vector2 shootVel = new Vector2(Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-14f, -9f));

                        int projType = ModContent.ProjectileType<WispFireBreath>();

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), tipPos, shootVel,
                            projType, (int)(NPC.damage * 0.5f), 0, -1);
                    }
                }
            }
            else if (AttackTimer > 150)
            {
                ResetState(ActionState.Idle);
            }
        }
        else if (sec == BaseAttack.CandleMash)
        {
        }
        else if (sec == BaseAttack.Fireblow)
        {
        }
    }

    private void ResetState(ActionState nextState)
    {
        AI_State = (float)nextState;
        AttackTimer = 0;
        AttackCount = 0;
        MainAttack = -1;
        SecAttack = -1;
        NPC.netUpdate = true;
    }

    public override void FindFrame(int frameHeight)
    {
        if (NPC.frameCounter++ >= 8)
        {
            faceFrameCurrent++;
            NPC.frameCounter = 0;
            if (faceFrameCurrent >= 4)
            {
                faceFrameCurrent = 0;
            }
        }
        base.FindFrame(frameHeight);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D outline = ModContent.Request<Texture2D>(Texture + "_Outline").Value;
        Texture2D face = ModContent.Request<Texture2D>(Texture + "_Face").Value;
        Rectangle frame = texture.Frame(1, 1, 0, 0);
        Rectangle frameFace = face.Frame(1, faceFrameTotal, 0, faceFrameCurrent);
        Texture2D glowOrb = Mod.Assets.Request<Texture2D>("Content/Projectiles/Friendly/Mage/TwilightDemiseHorribleThing").Value;
        Rectangle glowOrbFrame = glowOrb.Frame(1, 1, 0, 0);
        void DrawAtNPC(Texture2D tex, float scale)
        {
            sb.Draw(tex, NPC.Center + Main.rand.NextVector2Circular(2f, 2f) - Main.screenPosition, frame, Color.White * NPC.Opacity, NPC.rotation,
                new Vector2(tex.Width * 0.5f, tex.Height / Main.projFrames[Type] * 0.5f),
                scale, NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => Main.EntitySpriteDraw(glowOrb, NPC.Center + Main.rand.NextVector2Circular(1f, 1f) -
            Main.screenPosition, glowOrbFrame, new Color(131, 255, 236, 150), NPC.rotation, new Vector2(glowOrb.Width * 0.5f,
            glowOrb.Height / Main.projFrames[Type] * 0.5f), NPC.scale * 2f * MiscHelpers.BetterEssScale(2, 0.05f), SpriteEffects.None, 0f));

        emitter?.InjectDrawAction(ParticleEmitterDrawStep.BeforePreDrawAll, () => DrawAtNPC(outline, NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterPreDrawAll, () => DrawAtNPC(texture, NPC.scale));
        emitter?.InjectDrawAction(ParticleEmitterDrawStep.AfterDrawAll, () =>
            Main.EntitySpriteDraw(face, NPC.Center + new Vector2(0, 20 * NPC.scale) - Main.screenPosition, frameFace,
            Color.White * NPC.Opacity, NPC.rotation, frameFace.Size() / 2, NPC.scale, SpriteEffects.None));

        return false;
    }
}