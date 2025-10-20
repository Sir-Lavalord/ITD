using ITD.Content.Buffs.Debuffs;
using ITD.Utilities;
using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace ITD.Content.Projectiles.Friendly.Melee;

public class MoulderProj : ModProjectile
{
    private static Asset<Texture2D> chainTexture;
    private static Asset<Texture2D> chainTextureB;
    private static Asset<Texture2D> chainTextureC;
    private enum AIState
    {
        Spinning,
        LaunchingForward,
        Retracting
    }
    private AIState CurrentAIState
    {
        get => (AIState)Projectile.ai[0];
        set => Projectile.ai[0] = (float)value;
    }
    public ref float StateTimer => ref Projectile.ai[1];
    public ref float ChargeLevel => ref Projectile.ai[2];
    public bool Unlatched => Projectile.localAI[0] != 0;

    public ref float SpinningStateTimer => ref Projectile.localAI[1];

    public override void Load()
    {
        chainTexture = ModContent.Request<Texture2D>(Texture + "ChainA");
        chainTextureB = ModContent.Request<Texture2D>(Texture + "ChainB");
        chainTextureC = ModContent.Request<Texture2D>(Texture + "ChainC");

    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 3;

        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.netImportant = true;
        Projectile.width = 32;
        Projectile.height = 40;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI()
    {

        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f)
        {
            Projectile.Kill();
            return;
        }
        if (Main.myPlayer == Projectile.owner && Main.mapFullscreen)
        {
            Projectile.Kill();
            return;
        }

        Vector2 mountedCenter = player.MountedCenter;
        bool shouldOwnerHitCheck = false;
        int launchTimeLimit = 14;
        float launchSpeed = 18f;
        float maxRetractSpeed = 30f;
        float retractAcceleration = 5f;
        int defaultHitCooldown = 10;
        int spinHitCooldown = 20;
        int movingHitCooldown = 10;
        float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
        launchSpeed *= meleeSpeedMultiplier;
        maxRetractSpeed *= meleeSpeedMultiplier;
        retractAcceleration *= meleeSpeedMultiplier;
        float launchRange = launchSpeed * launchTimeLimit;
        Projectile.localNPCHitCooldown = defaultHitCooldown;

        switch (CurrentAIState)
        {
            case AIState.Spinning:
                {
                    Projectile.tileCollide = false;
                    shouldOwnerHitCheck = true;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
                        player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
                        if (!player.channel && ChargeLevel > 0)
                        {
                            CurrentAIState = AIState.LaunchingForward;
                            StateTimer = 0f;
                            Projectile.velocity = unitVectorTowardsMouse * launchSpeed + (player.velocity * 0.5f);
                            Projectile.Center = mountedCenter;
                            Projectile.netUpdate = true;
                            Projectile.ResetLocalNPCHitImmunity();
                            Projectile.localNPCHitCooldown = movingHitCooldown;
                            SoundEngine.PlaySound(SoundID.Item69, player.Center);
                            break;
                        }
                    }
                    SpinningStateTimer += 1f;
                    Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);
                    if (SpinningStateTimer % 25 == 0 && ChargeLevel < 2f)
                    {
                        ChargeLevel++;
                        SoundEngine.PlaySound(SoundID.Item72, player.Center);
                        for (int i = 0; i < 4; i++)
                        {
                            Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Stone, 0, 0, 150, default, 1f + (0.5f * ChargeLevel));
                            dust.noGravity = true;
                            dust.velocity *= 2f;
                        }
                    }
                    if (Main.rand.NextBool(3))
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.t_Slime, 0, 0, 150, Color.Black, 1f);
                        dust.noGravity = false;
                    }
                    offsetFromPlayer.Y *= 0.8f;
                    if (offsetFromPlayer.Y * player.gravDir > 0f)
                    {
                        offsetFromPlayer.Y *= 0.5f;
                    }
                    Projectile.Center = mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY);
                    Projectile.velocity = Vector2.Zero;
                    Projectile.localNPCHitCooldown = spinHitCooldown;
                    break;
                }
            case AIState.LaunchingForward:
                {
                    Projectile.tileCollide = true;
                    bool shouldSwitchToRetracting = StateTimer++ >= launchTimeLimit;
                    if (shouldSwitchToRetracting)
                    {
                        if (ChargeLevel >= 2)
                        {
                            if (Main.myPlayer == Projectile.owner)
                            {
                                Projectile.localAI[0]++;
                                Projectile.netUpdate = true;
                                SoundEngine.PlaySound(SoundID.Item72, player.Center);
                                for (int i = 0; i < 3; i++)
                                {
                                    Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.t_Slime, 0, 0, 150, Color.Black, 1f + (0.5f * ChargeLevel));
                                    dust.noGravity = true;
                                    dust.velocity *= 2f;
                                }
                                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.oldVelocity, ModContent.ProjectileType<MoulderBoulderProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                            }
                        }
                        CurrentAIState = AIState.Retracting;
                        StateTimer = 3f;
                        Projectile.netUpdate = true;
                        Projectile.velocity *= 0f;
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    Projectile.localNPCHitCooldown = movingHitCooldown;
                    break;
                }
            case AIState.Retracting:
                {
                    Projectile.tileCollide = false;
                    if (StateTimer++ > 3f)
                    {
                        Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
                        if (Projectile.Distance(mountedCenter) <= maxRetractSpeed)
                        {
                            Projectile.Kill();
                            return;
                        }
                        Projectile.velocity *= 0.98f;
                        Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxRetractSpeed, retractAcceleration);
                        Vector2 target = Projectile.Center + Projectile.velocity;
                        Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
                        if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f)
                        {
                            Projectile.Kill();
                            return;
                        }
                    }
                    player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
                    break;
                }
        }

        Projectile.ownerHitCheck = shouldOwnerHitCheck;

        Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
        Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver2;

        Projectile.timeLeft = 2;
        player.heldProj = Projectile.whoAmI;
        player.SetDummyItemTime(3);
        player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
        if (Projectile.Center.X < mountedCenter.X)
        {
            player.itemRotation += (float)Math.PI;
        }
        player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.netUpdate = true;

        Player player = Main.LocalPlayer;
        float power = 8 * Utils.GetLerpValue(800f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
        player.GetITDPlayer().BetterScreenshake(6, power, power, false);
        if (CurrentAIState != AIState.Retracting)
        {
            if (ChargeLevel >= 2)
            {
                if (StateTimer >= 8)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.localAI[0]++;
                        Projectile.netUpdate = true;
                        SoundEngine.PlaySound(SoundID.Item72, player.Center);
                        for (int i = 0; i < 3; i++)
                        {
                            Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.t_Slime, 0, 0, 150, Color.Black, 1f + (0.5f * ChargeLevel));
                            dust.noGravity = true;
                            dust.velocity *= 2f;
                        }
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.oldVelocity, ModContent.ProjectileType<MoulderBoulderProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                }
            }
            StateTimer = 0f;
            Projectile.velocity *= 0f;
            CurrentAIState = AIState.Retracting;
            for (int i = 0; i < 3; i++)
            {
                Collision.HitTiles(Projectile.position, oldVelocity, Projectile.width, Projectile.height);
            }

            for (int i = 0; i < 7; i++)
            {
                Vector2 randVelocity = oldVelocity * 0.5f * Main.rand.NextFloat();
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Stone, randVelocity.X, randVelocity.Y, 150, default, 1f + (0.5f * ChargeLevel));
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        return false;
    }

    public override bool? CanDamage()
    {
        if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f)
        {
            return false;
        }
        return base.CanDamage();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (CurrentAIState == AIState.Spinning)
        {
            Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
            shortestVectorFromPlayerToTarget.Y /= 0.8f;
            float hitRadius = 55f;
            return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
        }
        return base.Colliding(projHitbox, targetHitbox);
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 300);
        base.OnHitPlayer(target, info);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 300);
        base.OnHitNPC(target, hit, damageDone);
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.SourceDamage *= 1.2f;
        }
        else if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Retracting)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector2 randVelocity = Projectile.velocity * 0.5f * Main.rand.NextFloat();
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Stone, randVelocity.X, randVelocity.Y, 150, default, 1f + (0.5f * ChargeLevel));
                dust.noGravity = true;
                dust.velocity *= 2f;
            }

            modifiers.SourceDamage *= 1 + (0.5f * ChargeLevel);
            modifiers.Knockback *= 1 + ChargeLevel;
        }

        modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

        if (CurrentAIState == AIState.Spinning)
        {
            modifiers.Knockback *= 0.25f;
        }

        if (CurrentAIState == AIState.LaunchingForward)
        {
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);

        playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

        Rectangle? chainSourceRectangle = null;
        float chainHeightAdjustment = 0f;

        Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
        Vector2 chainDrawPosition = Projectile.Center;
        Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
        Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
        float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
        if (chainSegmentLength == 0)
        {
            chainSegmentLength = 18;
        }
        float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
        int chainCount = 0;
        float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;

        while (chainLengthRemainingToDraw > 0f)
        {
            var chainTextureToDraw = chainTexture;

            if (chainCount >= 6)
            {
                chainTextureToDraw = chainTextureC;
            }
            else if (chainCount >= 3)
            {
                chainTextureToDraw = chainTextureB;
            }
            else if (chainCount < 3)
            {
                chainTextureToDraw = chainTexture;
            }
            Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));
            Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);
            chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
            chainCount++;
            chainLengthRemainingToDraw -= chainSegmentLength;
        }


        Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
        if (CurrentAIState == AIState.Retracting && ChargeLevel >= 2 && Unlatched)
        {
            projectileTexture = ModContent.Request<Texture2D>(ITD.BlankTexture).Value;
        }
        Vector2 drawOrigin = new(projectileTexture.Width * 0.5f, projectileTexture.Height * 0.5f);
        if (CurrentAIState == AIState.LaunchingForward)
        {
            for (int k = 0; k < Projectile.oldPos.Length && k < StateTimer; k++)
            {
                Vector2 trailPos = Projectile.oldPos[k] - Main.screenPosition + (Projectile.Size * 0.5f) + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.spriteBatch.Draw(projectileTexture, trailPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, SpriteEffects.None, 0f);
            }
        }
        Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
        Main.spriteBatch.Draw(projectileTexture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}
